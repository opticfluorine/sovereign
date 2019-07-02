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

        public const int HMAC_SIZE_BYTES = 8;

        public const int MAX_PAYLOAD_SIZE = 1500 - HMAC_SIZE_BYTES;

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
        public Event DeserializeEvent(NetworkConnection connection, byte[] packetData)
        {
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

}
