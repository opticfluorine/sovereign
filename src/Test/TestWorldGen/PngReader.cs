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

using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;
using Sovereign.WorldGen.Output;

namespace TestWorldGen;

/// <summary>
///     A decoded truecolor RGB image.
/// </summary>
public sealed class DecodedImage
{
    /// <summary>
    ///     Width of the image in pixels.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    ///     Height of the image in pixels.
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    ///     Packed RGB pixel data in row-major order, top row first.
    /// </summary>
    public required byte[] Pixels { get; init; }
}

/// <summary>
///     Minimal PNG reader for PngWriter round-trip tests. Decodes the non-interlaced,
///     8-bit truecolor RGB images produced by <see cref="PngWriter" />.
/// </summary>
public static class PngReader
{
    /// <summary>
    ///     PNG file signature.
    /// </summary>
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    /// <summary>
    ///     Decodes a truecolor RGB PNG written by <see cref="PngWriter" />.
    /// </summary>
    /// <param name="path">Path of the PNG file.</param>
    /// <returns>Decoded image.</returns>
    public static DecodedImage Read(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        var signature = reader.ReadBytes(Signature.Length);
        if (!signature.AsSpan().SequenceEqual(Signature))
        {
            throw new InvalidDataException("Not a PNG file.");
        }

        int width = 0, height = 0;
        var idat = new MemoryStream();
        while (true)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4));
            var type = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var data = reader.ReadBytes(length);
            reader.ReadBytes(4); // CRC

            switch (type)
            {
                case "IHDR":
                    width = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(0, 4));
                    height = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(4, 4));
                    if (data[8] != 8 || data[9] != 2 || data[11] != 0 || data[12] != 0)
                    {
                        throw new InvalidDataException("Unsupported PNG format.");
                    }

                    break;

                case "IDAT":
                    idat.Write(data);
                    break;

                case "IEND":
                    return Decode(width, height, idat.ToArray());
            }
        }
    }

    /// <summary>
    ///     Inflates the image data and strips the per-row filter bytes.
    /// </summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="idat">Concatenated IDAT payloads.</param>
    /// <returns>Decoded image.</returns>
    private static DecodedImage Decode(int width, int height, byte[] idat)
    {
        using var source = new MemoryStream(idat);
        using var zlib = new ZLibStream(source, System.IO.Compression.CompressionMode.Decompress);
        using var inflated = new MemoryStream();
        zlib.CopyTo(inflated);

        var stride = width * 3;
        var expected = (stride + 1) * height;
        if (inflated.Length != expected)
        {
            throw new InvalidDataException(
                $"Expected {expected} inflated bytes, but decoded {inflated.Length}.");
        }

        var pixels = new byte[width * height * 3];
        var raw = inflated.ToArray();
        for (var y = 0; y < height; ++y)
        {
            if (raw[y * (stride + 1)] != 0)
            {
                throw new InvalidDataException("Unsupported row filter.");
            }

            raw.AsSpan(y * (stride + 1) + 1, stride).CopyTo(pixels.AsSpan(y * stride, stride));
        }

        return new DecodedImage { Width = width, Height = height, Pixels = pixels };
    }
}
