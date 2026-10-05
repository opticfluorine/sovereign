// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Sampling mode for a dynamic GUI texture.
/// </summary>
public enum DynamicTextureSampling
{
    /// <summary>
    ///     Nearest-neighbor sampling. Best for pixel art.
    /// </summary>
    Point,

    /// <summary>
    ///     Bilinear sampling. Best for photographic or high-resolution images
    ///     that may be displayed at a scaled size.
    /// </summary>
    Linear
}

/// <summary>
///     Manages dynamically updated textures that can be drawn in the GUI.
/// </summary>
/// <remarks>
///     <para>
///         All methods are thread-safe and may be called from any thread. GPU resources
///         are created and modified on the render thread at the start of the next frame,
///         so updates take effect with up to one frame of latency.
///     </para>
///     <para>
///         Textures are always 32-bit linear RGBA (R8G8B8A8_UNorm). Dimensions are fixed
///         at creation time; to resize a texture, remove it and add a new one.
///     </para>
/// </remarks>
public interface IDynamicTextureManager
{
    /// <summary>
    ///     Event raised on the render thread when a texture is removed.
    ///     The parameter is the handle of the removed texture.
    /// </summary>
    event Action<int> TextureRemoved;

    /// <summary>
    ///     Adds a new texture. The texture is initially filled with transparent black.
    /// </summary>
    /// <param name="width">Width in pixels. Must be positive.</param>
    /// <param name="height">Height in pixels. Must be positive.</param>
    /// <param name="sampling">Sampling mode used when the texture is drawn scaled.</param>
    /// <returns>Handle for the new texture, valid immediately.</returns>
    int AddTexture(uint width, uint height, DynamicTextureSampling sampling);

    /// <summary>
    ///     Replaces the pixel contents of a texture.
    /// </summary>
    /// <param name="handle">Texture handle.</param>
    /// <param name="rgbaPixels">
    ///     Pixel data in linear RGBA8 format, tightly packed row-major,
    ///     exactly width * height * 4 bytes.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the handle is unknown or the pixel buffer has the wrong length.
    /// </exception>
    void UpdateTexture(int handle, ReadOnlySpan<byte> rgbaPixels);

    /// <summary>
    ///     Removes a texture and releases its GPU resources. Removing an unknown
    ///     handle is a no-op. Handles are never reused.
    /// </summary>
    /// <param name="handle">Texture handle.</param>
    void RemoveTexture(int handle);

    /// <summary>
    ///     Gets the dimensions of a texture.
    /// </summary>
    /// <param name="handle">Texture handle.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <returns>true if the handle is known, false otherwise.</returns>
    bool TryGetDimensions(int handle, out uint width, out uint height);
}
