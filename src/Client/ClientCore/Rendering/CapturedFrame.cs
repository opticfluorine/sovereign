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

namespace Sovereign.ClientCore.Rendering;

/// <summary>
///     Pixel format of a captured frame.
/// </summary>
public enum CapturedPixelFormat
{
    /// <summary>
    ///     8-bit blue, green, red, alpha.
    /// </summary>
    Bgra8,

    /// <summary>
    ///     8-bit red, green, blue, alpha.
    /// </summary>
    Rgba8,

    /// <summary>
    ///     8-bit blue, green, red, alpha in the sRGB color space.
    /// </summary>
    Bgra8Srgb,

    /// <summary>
    ///     8-bit red, green, blue, alpha in the sRGB color space.
    /// </summary>
    Rgba8Srgb
}

/// <summary>
///     A captured frame of rendered output.
/// </summary>
public sealed class CapturedFrame
{
    /// <summary>
    ///     Width of the frame in pixels.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    ///     Height of the frame in pixels.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    ///     Pixel format of the frame data.
    /// </summary>
    public CapturedPixelFormat Format { get; init; }

    /// <summary>
    ///     Raw pixel data, ordered by row from the top of the frame, with no padding between rows.
    /// </summary>
    public byte[] Pixels { get; init; } = Array.Empty<byte>();
}
