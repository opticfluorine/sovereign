/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using ProtoBuf;
using Sovereign.EngineCore.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Sovereign.NetworkCore.Network.Infrastructure
{

    /// <summary>
    /// Responsible for serializing and deserializing network packets.
    /// </summary>
    public sealed class NetworkSerializer
    {

        public const int HMAC_SIZE_BYTES = 256 / 8;

        public const int MAX_PAYLOAD_SIZE = 1200;

        private readonly EventDescriptions eventDescriptions;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public NetworkSerializer(EventDescriptions eventDescriptions)
        {
            this.eventDescriptions = eventDescriptions;
        }

        /// <summary>
        /// Deserializes an event from a network packet.
        /// </summary>
        /// <param name="connection">Connection that sent the event.</param>
        /// <param name="packetData">Packet data.</param>
        /// <returns></returns>
        /// <exception cref="MalformedPacketException">
        /// Thrown if the packet contains bad data.
        /// </exception>
        /// <exception cref="BadHMACException">
        /// Thrown if the HMAC does not match the payload.
        /// </exception>
        /// <exception cref="BadNonceException">
        /// Thrown if a received nonce is reused, indicating a possible attempted
        /// replay attack.
        /// </exception>
        /// <exception cref="PacketSizeException">
        /// Thrown if the received packet is too large.
        /// </exception>
        public Event DeserializeEvent(NetworkConnection connection, byte[] packetData)
        {
            /* Validate packet size. */
            if (packetData.Length > MAX_PAYLOAD_SIZE + HMAC_SIZE_BYTES)
            {
                // Packet too large.
                throw new PacketSizeException("Packet of length " + packetData.Length
                    + " is too large.");
            }

            /* Unpack the raw data. */
            int payloadSize = packetData.Length - HMAC_SIZE_BYTES;
            if (payloadSize < 1)
            {
                /* Packet does not contain a payload. */
                throw new MalformedPacketException("No payload.");
            }
            var hmacSpan = new Span<byte>(packetData, 0, HMAC_SIZE_BYTES);
            var payloadSpan = new Span<byte>(packetData, HMAC_SIZE_BYTES, payloadSize);

            /* Verify the HMAC. */
            if (!VerifyHMAC(hmacSpan, payloadSpan, connection))
            {
                throw new BadHMACException("HMAC mismatch.");
            }

            /* Deserialize the payload. */
            return DeserializePayload(payloadSpan, connection);
        }

        /// <summary>
        /// Serializes an event to be sent over the network.
        /// </summary>
        /// <param name="connection">Network connection.</param>
        /// <param name="ev">Event to serialize.</param>
        /// <returns>Serialized event.</returns>
        /// <exception cref="PacketSizeException">
        /// Thrown if the serialized packet exceeds the maximum size.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the event type is not registered with EventDescriptions.
        /// </exception>
        /// <exception cref="MalformedEventException">
        /// Thrown if the event to be serialized is malformed.
        /// </exception>
        public byte[] SerializeEvent(NetworkConnection connection, Event ev)
        {
            // Determine how to serialize the event.
            Type detailType;
            detailType = eventDescriptions.GetDetailType(ev.EventId);

            // Verify the existence of the details.
            if ((detailType != null && ev.EventDetails == null) ||
                (detailType == null && ev.EventDetails != null))
            {
                throw new MalformedEventException("Unexpected event details.");
            }

            // If present, serialize the event details.
            byte[] innerPayload = SerializeDetails(detailType, ev.EventDetails);

            // Serialize the outer payload.
            byte[] outerPayload = SerializePayload(innerPayload, ev, connection);

            // Assemble the packet.
            var packet = new byte[outerPayload.Length + HMAC_SIZE_BYTES];
            AssemblePacket(packet, outerPayload, connection);
            return packet;
        }

        /// <summary>
        /// Assembles a packet, including the HMAC digest.
        /// </summary>
        /// <param name="packet">Buffer to hold the packet.</param>
        /// <param name="outerPayload">Serialized payload.</param>
        /// <param name="connection">Connection.</param>
        private void AssemblePacket(byte[] packet, byte[] outerPayload, NetworkConnection connection)
        {
            // Compute the HMAC digest for the packet.
            var srcPayloadSpan = new Span<byte>(outerPayload);
            var hmac = ComputeHMAC(srcPayloadSpan, connection);
            Array.Copy(hmac, 0, packet, 0, hmac.Length);

            // Fill the rest of the packet.
            Array.Copy(outerPayload, 0, packet, hmac.Length, outerPayload.Length);
        }

        /// <summary>
        /// Serializes the packet payload.
        /// </summary>
        /// <param name="innerPayload">Inner (details) payload.</param>
        /// <param name="ev">Event being serialized.</param>
        /// <param name="connection">Connection.</param>
        /// <returns>Serialized packet.</returns>
        /// <exception cref="PacketSizeException">
        /// Thrown if the serialized packet exceeds the maximum packet size.
        /// </exception>
        private byte[] SerializePayload(byte[] innerPayload, Event ev, NetworkConnection connection)
        {
            // Assemble the payload.
            var payload = new NetworkPayload();
            payload.EventId = ev.EventId;
            payload.Nonce = connection.GetOutboundNonce();
            payload.SerializedDetails = innerPayload;

            // Serialize the payload.
            byte[] outerPayload;
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, payload);
                    outerPayload = stream.ToArray();
                }
            }
            catch (Exception e)
            {
                throw new MalformedEventException("Failed to serialize payload.", e);
            }

            // Verify payload size.
            if (outerPayload.Length > MAX_PAYLOAD_SIZE)
            {
                throw new PacketSizeException("Serialized packet size too large.");
            }

            return outerPayload;
        }

        /// <summary>
        /// Serializes the event details.
        /// </summary>
        /// <param name="detailType">Event details type.</param>
        /// <param name="eventDetails">Event details to serialize.</param>
        /// <returns>Serialized event details, or a zero-length array if there are no details.</returns>
        /// <exception cref="MalformedEventException">
        /// Thrown if the event details type is incorrect.
        /// </exception>
        private byte[] SerializeDetails(Type detailType, IEventDetails eventDetails)
        {
            // Handle null details.
            if (detailType == null)
            {
                return new byte[0];
            }

            // Verify details type.
            if (eventDetails.GetType() != detailType)
            {
                throw new MalformedEventException("Event details type is incorrect.");
            }

            // Serialize.
            using (var stream = new MemoryStream())
            {
                try
                {
                    Serializer.NonGeneric.Serialize(stream, eventDetails);
                }
                catch (Exception e)
                {
                    throw new MalformedEventException("Failed to serialize event details.", e);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a payload into the corresponding event.
        /// </summary>
        /// <param name="payloadSpan"></param>
        /// <returns></returns>
        /// <exception cref="MalformedPacketException">
        /// Thrown if the payload cannot be deserialized.
        /// </exception>
        /// <exception cref="BadNonceException">
        /// Thrown if a received nonce is reused.
        /// </exception>
        private Event DeserializePayload(Span<byte> payloadSpan, NetworkConnection connection)
        {
            // Packet payloads are deserialized in two passes.
            // First the payload is deserialized into a NetworkPayload object.
            // Next the inner payload is deserialized to the correct IEventDetails object.

            // Deserialize to a NetworkPayload.
            NetworkPayload networkPayload;
            try
            {
                using (var outerStream = new MemoryStream(payloadSpan.ToArray()))
                {
                    networkPayload = Serializer.Deserialize<NetworkPayload>(outerStream);
                }
            }
            catch (Exception e)
            {
                // Bad data or something, couldn't deserialize.
                throw new MalformedPacketException("Failed to deserialize payload.", e);
            }

            // Verify the received nonce.
            if (!connection.IsReceivedNonceValid(networkPayload.Nonce))
            {
                // Repeated nonce, possible attempted replay attack.
                throw new BadNonceException("Possible attempted replay attack.");
            }

            // Move on to the second pass.
            return DeserializeInnerPayload(networkPayload);
        }

        /// <summary>
        /// Deserializes the inner payload to the event details.
        /// </summary>
        /// <param name="networkPayload">Result of the first pass.</param>
        /// <returns>Deserialized event.</returns>
        private Event DeserializeInnerPayload(NetworkPayload networkPayload)
        {
            // Determine what type of details need to be deserialized.
            Type detailType;
            try
            {
                detailType = eventDescriptions.GetDetailType(networkPayload.EventId);
            }
            catch (Exception e)
            {
                // Bad event type.
                throw new MalformedPacketException("Unrecognized event ID.", e);
            }

            // Check that fields are not null.
            if (networkPayload.SerializedDetails == null)
            {
                // Incomplete packet.
                throw new MalformedPacketException("Incomplete packet.");
            }

            // Check length of the serialized details.
            if ((detailType != null && networkPayload.SerializedDetails.Length == 0)
                || (detailType == null && networkPayload.SerializedDetails.Length > 0))
            {
                // Bad details length.
                throw new MalformedPacketException("Bad length of serialized details.");
            }

            // Deserialize the details if needed.
            IEventDetails details = null;
            if (detailType != null)
            {
                try
                {
                    using (var stream = new MemoryStream(networkPayload.SerializedDetails))
                    {
                        details = (IEventDetails)Serializer.Deserialize(detailType, stream);
                    }
                }
                catch (Exception e)
                {
                    throw new MalformedPacketException("Failed to deserialize event details.", e);
                }
            }

            // Construct the event.
            const bool NONLOCAL = false;
            return new Event(networkPayload.EventId,
                details, Event.Immediate, NONLOCAL);
        }

        /// <summary>
        /// Checks the HMAC digest of the packet.
        /// </summary>
        /// <param name="hmacSpan">HMAC digest included in the packet.</param>
        /// <param name="payloadSpan">Packet payload.</param>
        /// <param name="connection">Associated connection.</param>
        /// <returns>true if the HMAC is valid, false otherwise.</returns>
        private bool VerifyHMAC(Span<byte> hmacSpan, Span<byte> payloadSpan, NetworkConnection connection)
        {
            /* Compute the HMAC for the payload. */
            var hmac = ComputeHMAC(payloadSpan, connection);
            if (hmac.Length != hmacSpan.Length)
            {
                Logger.Error("HMAC size mismatch.");
                return false;
            }

            // Compare all of the bytes of the HMACs.
            // Checking all bytes in all cases improves resistance to timing attacks.
            var delta = 0;
            for (int i = 0; i < hmac.Length; ++i)
            {
                if (hmacSpan[i] != hmac[i]) delta++;
            }
            return delta == 0;
        }

        /// <summary>
        /// Computes the HMAC digest of a packet.
        /// </summary>
        /// <param name="payloadSpan">Packet payload.</param>
        /// <param name="connection">Associated connection.</param>
        /// <returns></returns>
        private byte[] ComputeHMAC(Span<byte> payloadSpan, NetworkConnection connection)
        {
            using (var hmac = new HMACSHA256(connection.Key))
            {
                hmac.Initialize();
                return hmac.ComputeHash(payloadSpan.ToArray());
            }
        }

    }

    /// <summary>
    /// Exception type thrown when a malformed packet is encountered.
    /// </summary>
    public sealed class MalformedPacketException : Exception
    {
        public MalformedPacketException()
        {
        }

        public MalformedPacketException(string message) : base(message)
        {
        }

        public MalformedPacketException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception type thrown when an event is malformed.
    /// </summary>
    public sealed class MalformedEventException : Exception
    {
        public MalformedEventException()
        {
        }

        public MalformedEventException(string message) : base(message)
        {
        }

        public MalformedEventException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception type thrown when an HMAC does not match the payload.
    /// </summary>
    public sealed class BadHMACException : Exception
    {
        public BadHMACException()
        {
        }

        public BadHMACException(string message) : base(message)
        {
        }

        public BadHMACException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception type thrown when a received nonce is reused, indicating a
    /// possible attempted replay attack.
    /// </summary>
    public sealed class BadNonceException : Exception
    {
        public BadNonceException()
        {
        }

        public BadNonceException(string message) : base(message)
        {
        }

        public BadNonceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class PacketSizeException : Exception
    {
        public PacketSizeException()
        {
        }

        public PacketSizeException(string message) : base(message)
        {
        }

        public PacketSizeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
