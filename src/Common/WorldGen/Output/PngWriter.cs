// Sovereign Engine
// Copyright (c) 2026 opticfluorine
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace Sovereign.WorldGen.Output;

/// <summary>
///     Minimal truecolor RGB PNG encoder producing non-interlaced 8-bit images with one
///     uncompressed-row IDAT stream (filter type 0 per row), intended for world generation
///     preview images.
/// </summary>
public static class PngWriter
{
    /// <summary>
    ///     PNG file signature.
    /// </summary>
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    /// <summary>
    ///     Precomputed CRC-32 lookup table.
    /// </summary>
    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>
    ///     Writes a truecolor RGB PNG image to a stream.
    /// </summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="rgb">Pixel data as packed RGB triples in row-major order, top row first;
    /// must contain exactly width * height * 3 bytes.</param>
    public static void WritePng(Stream stream, int width, int height, ReadOnlySpan<byte> rgb)
    {
        if (rgb.Length != width * height * 3)
        {
            throw new ArgumentException(
                $"Pixel buffer must contain {width * height * 3} bytes, but has {rgb.Length}.");
        }

        stream.Write(Signature);
        WriteChunk(stream, "IHDR", HeaderData(width, height));

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, true))
        {
            WriteFilteredRows(zlib, width, height, rgb);
        }

        using var chunk = new MemoryStream();
        WriteChunkHeader(chunk, "IDAT", (int)compressed.Length);
        compressed.Position = 0;
        compressed.CopyTo(chunk);
        WriteChunkWithHeader(stream, chunk, "IDAT");

        WriteChunk(stream, "IEND", ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    ///     Writes a truecolor RGB PNG image to a file.
    /// </summary>
    /// <param name="path">Destination file path.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="rgb">Pixel data as packed RGB triples in row-major order, top row first;
    /// must contain exactly width * height * 3 bytes.</param>
    public static void WritePng(string path, int width, int height, ReadOnlySpan<byte> rgb)
    {
        using var stream = File.Create(path);
        WritePng(stream, width, height, rgb);
    }

    /// <summary>
    ///     Builds the IHDR chunk payload.
    /// </summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <returns>IHDR payload.</returns>
    private static byte[] HeaderData(int width, int height)
    {
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 8; // bit depth
        header[9] = 2; // color type: truecolor RGB
        header[10] = 0; // compression: deflate
        header[11] = 0; // filter method
        header[12] = 0; // interlace: none
        return header;
    }

    /// <summary>
    ///     Writes the pixel rows prefixed with filter type 0.
    /// </summary>
    /// <param name="destination">Destination stream.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="rgb">Pixel data.</param>
    private static void WriteFilteredRows(Stream destination, int width, int height, ReadOnlySpan<byte> rgb)
    {
        var stride = width * 3;
        var row = new byte[stride + 1];
        for (var y = 0; y < height; ++y)
        {
            row[0] = 0; // filter type: none
            rgb.Slice(y * stride, stride).CopyTo(row.AsSpan(1));
            destination.Write(row);
        }
    }

    /// <summary>
    ///     Writes one PNG chunk with type and payload.
    /// </summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="type">Chunk type as four ASCII characters.</param>
    /// <param name="data">Chunk payload.</param>
    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        using var chunk = new MemoryStream();
        WriteChunkHeader(chunk, type, data.Length);
        chunk.Write(data);
        WriteChunkWithHeader(stream, chunk, type);
    }

    /// <summary>
    ///     Writes a chunk header: payload length and chunk type.
    /// </summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="type">Chunk type as four ASCII characters.</param>
    /// <param name="length">Payload length in bytes.</param>
    private static void WriteChunkHeader(Stream stream, string type, int length)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt32BigEndian(buffer, length);
        WriteType(buffer.Slice(4), type);
        stream.Write(buffer);
    }

    /// <summary>
    ///     Writes a chunk payload previously staged with its header, followed by the CRC.
    /// </summary>
    /// <param name="stream">Destination stream.</param>
    /// <param name="chunk">Staged chunk with header and payload.</param>
    /// <param name="type">Chunk type as four ASCII characters.</param>
    private static void WriteChunkWithHeader(Stream stream, MemoryStream chunk, string type)
    {
        chunk.Position = 0;
        chunk.CopyTo(stream);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, CrcOf(chunk.GetBuffer(), (int)chunk.Length, type));
        stream.Write(crc);
    }

    /// <summary>
    ///     Writes a chunk type as four ASCII bytes.
    /// </summary>
    /// <param name="buffer">Buffer to receive the type; must be at least four bytes.</param>
    /// <param name="type">Chunk type string.</param>
    private static void WriteType(Span<byte> buffer, string type)
    {
        for (var i = 0; i < 4; ++i)
        {
            buffer[i] = (byte)type[i];
        }
    }

    /// <summary>
    ///     Computes the CRC-32 of a chunk over its type and payload.
    /// </summary>
    /// <param name="staged">Staged chunk with header and payload.</param>
    /// <param name="length">Staged length in bytes.</param>
    /// <param name="type">Chunk type string.</param>
    /// <returns>CRC-32 value.</returns>
    private static uint CrcOf(ReadOnlySpan<byte> staged, int length, string type)
    {
        var crc = UpdateCrc(0xFFFFFFFF, type);
        crc = UpdateCrc(crc, staged.Slice(8, length - 8));
        return crc ^ 0xFFFFFFFF;
    }

    /// <summary>
    ///     Updates a CRC-32 value over a byte span.
    /// </summary>
    /// <param name="initial">Initial CRC value.</param>
    /// <param name="data">Data span.</param>
    /// <returns>Updated CRC value.</returns>
    private static uint UpdateCrc(uint initial, ReadOnlySpan<byte> data)
    {
        var crc = initial;
        foreach (var b in data)
        {
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    /// <summary>
    ///     Updates a CRC-32 value over a string.
    /// </summary>
    /// <param name="initial">Initial CRC value.</param>
    /// <param name="text">Text to hash.</param>
    /// <returns>Updated CRC value.</returns>
    private static uint UpdateCrc(uint initial, string text)
    {
        var crc = initial;
        foreach (var c in text)
        {
            crc = CrcTable[(crc ^ (byte)c) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    /// <summary>
    ///     Builds the CRC-32 lookup table.
    /// </summary>
    /// <returns>Lookup table.</returns>
    private static uint[] BuildCrcTable()
    {
        const uint polynomial = 0xEDB88320;
        var table = new uint[256];
        for (var i = 0; i < 256; ++i)
        {
            var value = (uint)i;
            for (var bit = 0; bit < 8; ++bit)
            {
                value = (value & 1) != 0 ? polynomial ^ (value >> 1) : value >> 1;
            }

            table[i] = value;
        }

        return table;
    }
}
