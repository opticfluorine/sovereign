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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Castle.Core.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;

namespace Sovereign.NetworkCore.Network.Infrastructure;

/// <summary>
///     Responsible for serializing and deserializing network packets.
/// </summary>
public sealed class NetworkSerializer
{
    public const int HMAC_SIZE_BYTES = 256 / 8;

    public const int MAX_PAYLOAD_SIZE = 1200;

    private readonly EventDescriptions eventDescriptions;

    public NetworkSerializer(EventDescriptions eventDescriptions)
    {
        this.eventDescriptions = eventDescriptions;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Deserializes an event from a network packet.
    /// </summary>
    /// <param name="connection">Connection that sent the event.</param>
    /// <param name="packetData">Packet data.</param>
    /// <returns></returns>
    /// <exception cref="MalformedPacketException">
    ///     Thrown if the packet contains bad data.
    /// </exception>
    /// <exception cref="BadHmacException">
    ///     Thrown if the HMAC does not match the payload.
    /// </exception>
    /// <exception cref="BadNonceException">
    ///     Thrown if a received nonce is reused, indicating a possible attempted
    ///     replay attack.
    /// </exception>
    /// <exception cref="PacketSizeException">
    ///     Thrown if the received packet is too large.
    /// </exception>
    public Event DeserializeEvent(NetworkConnection connection, byte[] packetData)
    {
        /* Validate packet size. */
        if (packetData.Length > MAX_PAYLOAD_SIZE + HMAC_SIZE_BYTES)
            // Packet too large.
            throw new PacketSizeException("Packet of length " + packetData.Length
                                                              + " is too large.");

        /* Unpack the raw data. */
        var payloadSize = packetData.Length - HMAC_SIZE_BYTES;
        if (payloadSize < 1)
            /* Packet does not contain a payload. */
            throw new MalformedPacketException("No payload.");
        var hmacSpan = new Span<byte>(packetData, 0, HMAC_SIZE_BYTES);
        var payloadSpan = new Span<byte>(packetData, HMAC_SIZE_BYTES, payloadSize);

        /* Verify the HMAC. */
        if (!VerifyHmac(hmacSpan, payloadSpan, connection)) throw new BadHmacException("HMAC mismatch.");

        /* Deserialize the payload. */
        return DeserializePayload(payloadSpan, connection);
    }

    /// <summary>
    ///     Serializes an event to be sent over the network.
    /// </summary>
    /// <param name="connection">Network connection.</param>
    /// <param name="ev">Event to serialize.</param>
    /// <returns>Serialized event.</returns>
    /// <exception cref="PacketSizeException">
    ///     Thrown if the serialized packet exceeds the maximum size.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if the event type is not registered with EventDescriptions.
    /// </exception>
    /// <exception cref="MalformedEventException">
    ///     Thrown if the event to be serialized is malformed.
    /// </exception>
    public byte[] SerializeEvent(NetworkConnection connection, Event ev)
    {
        // Serialize the event.
        var payload = SerializeEvent(ev, connection);

        // Assemble the packet.
        var packet = new byte[payload.Length + HMAC_SIZE_BYTES];
        AssemblePacket(packet, payload, connection);
        return packet;
    }

    /// <summary>
    ///     Assembles a packet, including the HMAC digest.
    /// </summary>
    /// <param name="packet">Buffer to hold the packet.</param>
    /// <param name="payload">Serialized payload.</param>
    /// <param name="connection">Connection.</param>
    private void AssemblePacket(byte[] packet, byte[] payload, NetworkConnection connection)
    {
        // Compute the HMAC digest for the packet.
        var srcPayloadSpan = new Span<byte>(payload);
        var hmac = ComputeHmac(srcPayloadSpan, connection);
        Array.Copy(hmac, 0, packet, 0, hmac.Length);

        // Fill the rest of the packet.
        Array.Copy(payload, 0, packet, hmac.Length, payload.Length);
    }

    /// <summary>
    ///     Serializes the packet payload.
    /// </summary>
    /// <param name="ev">Event being serialized.</param>
    /// <param name="connection">Connection.</param>
    /// <returns>Serialized packet.</returns>
    /// <exception cref="PacketSizeException">
    ///     Thrown if the serialized packet exceeds the maximum packet size.
    /// </exception>
    private byte[] SerializeEvent(Event ev, NetworkConnection connection)
    {
        // Assemble the payload.
        var payload = new NetworkPayload(connection.GetOutboundNonce(), ev);

        // Serialize the payload.
        var payloadBytes = MessageConfig.SerializeMsgPack(payload);

        // Verify payload size.
        if (payloadBytes.Length > MAX_PAYLOAD_SIZE) throw new PacketSizeException("Serialized packet size too large.");

        return payloadBytes;
    }

    /// <summary>
    ///     Deserializes a payload into the corresponding event.
    /// </summary>
    /// <param name="payloadSpan"></param>
    /// <returns></returns>
    /// <exception cref="MalformedPacketException">
    ///     Thrown if the payload cannot be deserialized.
    /// </exception>
    /// <exception cref="BadNonceException">
    ///     Thrown if a received nonce is reused.
    /// </exception>
    private Event DeserializePayload(Span<byte> payloadSpan, NetworkConnection connection)
    {
        // Packet payloads are deserialized in two passes.
        // First the payload is deserialized into a NetworkPayload object.
        // Next the inner payload is deserialized to the correct IEventDetails object.

        // Deserialize to a NetworkPayload.
        NetworkPayload payload;
        try
        {
            payload = MessageConfig.DeserializeMsgPack<NetworkPayload>(payloadSpan.ToArray());
        }
        catch (Exception e)
        {
            throw new MalformedPacketException("Could not deserialize event.", e);
        }

        // Verify the received nonce.
        if (!connection.IsReceivedNonceValid(payload.Nonce))
            // Repeated nonce, possible attempted replay attack.
            throw new BadNonceException("Possible attempted replay attack.");

        // All good.
        payload.Event.Local = false;
        return payload.Event;
    }

    /// <summary>
    ///     Checks the HMAC digest of the packet.
    /// </summary>
    /// <param name="hmacSpan">HMAC digest included in the packet.</param>
    /// <param name="payloadSpan">Packet payload.</param>
    /// <param name="connection">Associated connection.</param>
    /// <returns>true if the HMAC is valid, false otherwise.</returns>
    private bool VerifyHmac(Span<byte> hmacSpan, Span<byte> payloadSpan, NetworkConnection connection)
    {
        /* Compute the HMAC for the payload. */
        var hmac = ComputeHmac(payloadSpan, connection);
        if (hmac.Length != hmacSpan.Length)
        {
            Logger.Error("HMAC size mismatch.");
            return false;
        }

        // Compare all of the bytes of the HMACs.
        // Checking all bytes in all cases improves resistance to timing attacks.
        var delta = 0;
        for (var i = 0; i < hmac.Length; ++i)
            if (hmacSpan[i] != hmac[i])
                delta++;
        return delta == 0;
    }

    /// <summary>
    ///     Computes the HMAC digest of a packet.
    /// </summary>
    /// <param name="payloadSpan">Packet payload.</param>
    /// <param name="connection">Associated connection.</param>
    /// <returns></returns>
    private byte[] ComputeHmac(Span<byte> payloadSpan, NetworkConnection connection)
    {
        using (var hmac = new HMACSHA256(connection.Key))
        {
            hmac.Initialize();
            return hmac.ComputeHash(payloadSpan.ToArray());
        }
    }
}

/// <summary>
///     Exception type thrown when a malformed packet is encountered.
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
///     Exception type thrown when an event is malformed.
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
///     Exception type thrown when an HMAC does not match the payload.
/// </summary>
public sealed class BadHmacException : Exception
{
    public BadHmacException()
    {
    }

    public BadHmacException(string message) : base(message)
    {
    }

    public BadHmacException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Exception type thrown when a received nonce is reused, indicating a
///     possible attempted replay attack.
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