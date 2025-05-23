/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Network;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.NetworkCore.Network.Infrastructure;

/// <summary>
///     Responsible for serializing and deserializing network packets.
/// </summary>
public sealed class NetworkSerializer
{
    public const int HmacSizeBytes = 128 / 8;

    public const int MaxPayloadSize = 1200;

    private readonly ObjectPool<BufferPair> bufferPool = new();

    private readonly ObjectPool<HMACSHA512> hmacPool = new();
    private readonly ILogger<NetworkSerializer> logger;

    public NetworkSerializer(ILogger<NetworkSerializer> logger)
    {
        this.logger = logger;
    }

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
        if (packetData.Length > MaxPayloadSize + HmacSizeBytes)
            // Packet too large.
            throw new PacketSizeException("Packet of length " + packetData.Length
                                                              + " is too large.");

        /* Unpack the raw data. */
        var payloadSize = packetData.Length - HmacSizeBytes;
        if (payloadSize < 1)
            /* Packet does not contain a payload. */
            throw new MalformedPacketException("No payload.");
        var bufferPair = bufferPool.TakeObject();
        try
        {
            Array.Copy(packetData, bufferPair.HmacPart, HmacSizeBytes);
            Array.Fill(bufferPair.PayloadPart, (byte)0);
            Array.Copy(packetData, HmacSizeBytes, bufferPair.PayloadPart, 0, payloadSize);

            /* Verify the HMAC. */
            if (!VerifyHmac(bufferPair.HmacPart, bufferPair.PayloadPart, payloadSize, connection))
                throw new BadHmacException("HMAC mismatch.");

            /* Deserialize the payload. */
            return DeserializePayload(bufferPair.PayloadPart, connection);
        }
        finally
        {
            bufferPool.ReturnObject(bufferPair);
        }
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
        var packet = new byte[payload.Length + HmacSizeBytes];
        AssemblePacket(packet, payload, payload.Length, connection);
        return packet;
    }

    /// <summary>
    ///     Assembles a packet, including the HMAC digest.
    /// </summary>
    /// <param name="packet">Buffer to hold the packet.</param>
    /// <param name="payload">Serialized payload.</param>
    /// <param name="payloadLength">Length of payload in bytes.</param>
    /// <param name="connection">Connection.</param>
    private void AssemblePacket(byte[] packet, byte[] payload, int payloadLength, NetworkConnection connection)
    {
        // Compute the HMAC digest for the packet.
        var hmac = ComputeHmac(payload, payloadLength, connection);
        Array.Copy(hmac, 0, packet, 0, HmacSizeBytes);

        // Fill the rest of the packet.
        Array.Copy(payload, 0, packet, HmacSizeBytes, payload.Length);
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
        if (payloadBytes.Length > MaxPayloadSize) throw new PacketSizeException("Serialized packet size too large.");

        return payloadBytes;
    }

    /// <summary>
    ///     Deserializes a payload into the corresponding event.
    /// </summary>
    /// <param name="payloadBytes">Payload.</param>
    /// <param name="connection">Connection.</param>
    /// <returns></returns>
    /// <exception cref="MalformedPacketException">
    ///     Thrown if the payload cannot be deserialized.
    /// </exception>
    /// <exception cref="BadNonceException">
    ///     Thrown if a received nonce is reused.
    /// </exception>
    private Event DeserializePayload(byte[] payloadBytes, NetworkConnection connection)
    {
        // Packet payloads are deserialized in two passes.
        // First the payload is deserialized into a NetworkPayload object.
        // Next the inner payload is deserialized to the correct IEventDetails object.

        // Deserialize to a NetworkPayload.
        NetworkPayload payload;
        try
        {
            payload = MessageConfig.DeserializeMsgPack<NetworkPayload>(payloadBytes);
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
        payload.Event.FromConnectionId = connection.Id;
        return payload.Event;
    }

    /// <summary>
    ///     Checks the HMAC digest of the packet.
    /// </summary>
    /// <param name="hmacSpan">HMAC digest included in the packet.</param>
    /// <param name="payloadBytes">Packet payload.</param>
    /// <param name="payloadLength">Length of payload in bytes.</param>
    /// <param name="connection">Associated connection.</param>
    /// <returns>true if the HMAC is valid, false otherwise.</returns>
    private bool VerifyHmac(byte[] hmacSpan, byte[] payloadBytes, int payloadLength, NetworkConnection connection)
    {
        /* Compute the HMAC for the payload. */
        var hmac = ComputeHmac(payloadBytes, payloadLength, connection);
        if (hmac.Length < hmacSpan.Length)
        {
            logger.LogError("HMAC size mismatch.");
            return false;
        }

        // Compare all of the bytes of the HMACs (up to truncation).
        // Checking all bytes in all cases improves resistance to timing attacks.
        var delta = 0;
        for (var i = 0; i < hmacSpan.Length; ++i)
            if (hmacSpan[i] != hmac[i])
                delta++;
        return delta == 0;
    }

    /// <summary>
    ///     Computes the HMAC digest of a packet.
    /// </summary>
    /// <param name="payloadBytes">Packet payload.</param>
    /// <param name="payloadLength">Length of payload in bytes.</param>
    /// <param name="connection">Associated connection.</param>
    /// <returns></returns>
    private byte[] ComputeHmac(byte[] payloadBytes, int payloadLength, NetworkConnection connection)
    {
        var hmac = hmacPool.TakeObject();
        try
        {
            hmac.Key = connection.Key;
            hmac.Initialize();
            return hmac.ComputeHash(payloadBytes, 0, payloadLength);
        }
        finally
        {
            hmacPool.ReturnObject(hmac);
        }
    }

    /// <summary>
    ///     Reusable pair of network buffers.
    /// </summary>
    private class BufferPair
    {
        /// <summary>
        ///     HMAC byte buffer.
        /// </summary>
        public readonly byte[] HmacPart = new byte[HmacSizeBytes];

        /// <summary>
        ///     Payload byte buffer.
        /// </summary>
        public readonly byte[] PayloadPart = new byte[MaxPayloadSize];
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