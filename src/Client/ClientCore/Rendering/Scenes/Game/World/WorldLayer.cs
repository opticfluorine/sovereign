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

using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Organizes a single layer of the world for rendering.
/// </summary>
public sealed class WorldLayer
{
    /// <summary>
    ///     Initial number of tile sprite records to allocate.
    /// </summary>
    public const int InitialTileSpriteCount = 2048;

    /// <summary>
    ///     Initial number of animated sprite records to allocate.
    /// </summary>
    public const int InitialAnimatedSpriteCount = 512;

    /// <summary>
    ///     Minimum z value contained by this layer.
    /// </summary>
    public int ZFloor { get; set; }

    /// <summary>
    ///     Tile sprites forming the floor of this layer.
    /// </summary>
    public IList<PosVelId> TopFaceTileSprites { get; }
        = new List<PosVelId>(InitialTileSpriteCount);

    /// <summary>
    ///     Front face tile sprites of blocks in the next higher layer.
    /// </summary>
    public IList<PosVelId> FrontFaceTileSprites { get; }
        = new List<PosVelId>(InitialTileSpriteCount);

    /// <summary>
    ///     Animated sprites to additionally be drawn in this layer.
    /// </summary>
    public IList<PosVelId> AnimatedSprites { get; }
        = new List<PosVelId>(InitialAnimatedSpriteCount);

    /// <summary>
    ///     Resets the layer.
    /// </summary>
    public void Reset()
    {
        TopFaceTileSprites.Clear();
        FrontFaceTileSprites.Clear();
        AnimatedSprites.Clear();
    }
}