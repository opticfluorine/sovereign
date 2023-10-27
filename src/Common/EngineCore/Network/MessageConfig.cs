/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

using System.Text.Json;
using MessagePack;

namespace Sovereign.EngineCore.Network;

/// <summary>
///     Provides common configuration for message handling.
/// </summary>
public sealed class MessageConfig
{
    /// <summary>
    ///     MessagePack options for untruested and compressed messages.
    /// </summary>
    public static MessagePackSerializerOptions CompressedUntrustedMessagePackOptions =>
        MessagePackSerializerOptions
            .Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);

    /// <summary>
    ///     MessagePack options for untrusted messages without compression.
    /// </summary>
    public static MessagePackSerializerOptions UntrustedMessagePackOptions =>
        MessagePackSerializerOptions
            .Standard
            .WithSecurity(MessagePackSecurity.UntrustedData);

    /// <summary>
    ///     JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

    /// <summary>
    ///     Utility method that serializes an object using MessagePack.
    ///     This applies default settings for untrusted data.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="compressed">Whether to compress the output.</param>
    /// <typeparam name="T">Type of object to serialize.</typeparam>
    /// <returns>Serialized object.</returns>
    public static byte[] SerializeMsgPack<T>(T obj, bool compressed = false)
    {
        return MessagePackSerializer.Serialize(obj,
            compressed ? CompressedUntrustedMessagePackOptions : UntrustedMessagePackOptions);
    }

    /// <summary>
    ///     Utility method that deserializes an object using MessagePack.
    ///     This applies default settings for untrusted data.
    /// </summary>
    /// <param name="data">Potentially untrusted data to deserialize.</param>
    /// <param name="compressed">Whether the object is compressed.</param>
    /// <typeparam name="T">Type of object to deserialize.</typeparam>
    /// <returns>Deserialized object.</returns>
    public static T DeserializeMsgPack<T>(byte[] data, bool compressed = false)
    {
        return MessagePackSerializer.Deserialize<T>(data,
            compressed ? CompressedUntrustedMessagePackOptions : UntrustedMessagePackOptions);
    }
}