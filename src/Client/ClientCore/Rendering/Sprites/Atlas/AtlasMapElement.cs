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

namespace Sovereign.ClientCore.Rendering.Sprites.Atlas;

/// <summary>
///     Encodes an entry in the texture atlas.
/// </summary>
public class AtlasMapElement
{
    /// <summary>
    ///     Height as a multiple of the tile width.
    /// </summary>
    public float HeightInTiles;

    /// <summary>
    ///     Normalized texel coordinate of the bottom edge.
    /// </summary>
    public float NormalizedBottomY;

    /// <summary>
    ///     Normalized texel coordinate of the left edge.
    /// </summary>
    public float NormalizedLeftX;

    /// <summary>
    ///     Normalized texel coordinate of the right edge.
    /// </summary>
    public float NormalizedRightX;

    /// <summary>
    ///     Normalized texel coordinate of the top edge.
    /// </summary>
    public float NormalizedTopY;

    /// <summary>
    ///     Width as a multiple of the tile width.
    /// </summary>
    public float WidthInTiles;
}