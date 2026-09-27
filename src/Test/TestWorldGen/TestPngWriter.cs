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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Sovereign.WorldGen.Output;
using Xunit;

namespace TestWorldGen;

/// <summary>
///     Structural tests for <see cref="PngWriter" /> output.
/// </summary>
public class TestPngWriter
{
    /// <summary>
    ///     Expected PNG file signature.
    /// </summary>
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    [Fact]
    public void WritePng_EmitsWellFormedTruecolorImage()
    {
        const int width = 7;
        const int height = 5;
        var pixels = new byte[width * height * 3];
        for (var i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = (byte)(i * 7);
        }

        using var stream = new MemoryStream();
        PngWriter.WritePng(stream, width, height, pixels);
        var bytes = stream.ToArray();

        Assert.Equal(Signature, bytes[..8]);

        var chunks = ReadChunks(bytes);
        Assert.Equal(new[] { "IHDR", "IDAT", "IEND" }, chunks.ConvertAll(c => c.Type));

        var ihdr = chunks[0];
        Assert.Equal(13, ihdr.Data.Length);
        Assert.Equal(width, BinaryPrimitives.ReadInt32BigEndian(ihdr.Data));
        Assert.Equal(height, BinaryPrimitives.ReadInt32BigEndian(ihdr.Data.AsSpan(4)));
        Assert.Equal(8, ihdr.Data[8]); // bit depth
        Assert.Equal(2, ihdr.Data[9]); // color type: truecolor RGB
        Assert.Equal(0, ihdr.Data[12]); // no interlace

        var idat = chunks[1];
        var raw = Inflate(idat.Data);
        Assert.Equal(height * (width * 3 + 1), raw.Length);
        for (var y = 0; y < height; ++y)
        {
            Assert.Equal(0, raw[y * (width * 3 + 1)]); // filter type 0 per row
        }

        Assert.Empty(chunks[2].Data);
    }

    [Fact]
    public void WritePng_BufferLengthMismatch_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PngWriter.WritePng(new MemoryStream(), 4, 4, new byte[10]));
    }

    /// <summary>
    ///     Reads the chunks of a PNG byte stream, verifying CRCs.
    /// </summary>
    /// <param name="bytes">PNG byte stream.</param>
    /// <returns>Chunks in file order.</returns>
    private static List<(string Type, byte[] Data)> ReadChunks(byte[] bytes)
    {
        var chunks = new List<(string, byte[])>();
        var pos = 8;
        while (pos < bytes.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(pos));
            var type = Encoding.ASCII.GetString(bytes, pos + 4, 4);
            var data = bytes[(pos + 8)..(pos + 8 + length)];
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(pos + 8 + length));
            Assert.Equal(expectedCrc, Crc32(bytes, pos + 4, length + 4));
            chunks.Add((type, data));
            pos += 12 + length;
        }

        return chunks;
    }

    /// <summary>
    ///     Inflates a zlib-compressed IDAT payload.
    /// </summary>
    /// <param name="data">Compressed payload.</param>
    /// <returns>Decompressed bytes.</returns>
    private static byte[] Inflate(byte[] data)
    {
        using var source = new MemoryStream(data);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>
    ///     Computes the CRC-32 over a span of the byte array.
    /// </summary>
    private static uint Crc32(byte[] bytes, int offset, int length)
    {
        uint crc = 0xFFFFFFFF;
        for (var i = offset; i < offset + length; ++i)
        {
            crc ^= bytes[i];
            for (var bit = 0; bit < 8; ++bit)
            {
                crc = (crc & 1) != 0 ? 0xEDB88320 ^ (crc >> 1) : crc >> 1;
            }
        }

        return crc ^ 0xFFFFFFFF;
    }
}
