/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

namespace Sovereign.ClientCore.Rendering.Configuration;

/// <summary>
///     Enumeration of possible display formats. These are also used for
///     internal pixel formats, so not all are suitable for display.
/// </summary>
public enum DisplayFormat
{
    /// <summary>
    ///     RGBA, 8 bits per channel, normalized unsigned integer.
    /// </summary>
    R8G8B8A8_UNorm,

    /// <summary>
    ///     BGRA, 8 bits per channel, normalized unsigned integer.
    /// </summary>
    B8G8R8A8_UNorm,

    /// <summary>
    ///     Represents any SDL-supported pixel format that is not a valid
    ///     display format. These occur in loading sprite resources; conversion
    ///     to a display-suitable format occurs when the spritesheets are
    ///     blitted onto a new texture atlas surface.
    /// </summary>
    CpuUseOnly
}