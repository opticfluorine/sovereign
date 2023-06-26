/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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