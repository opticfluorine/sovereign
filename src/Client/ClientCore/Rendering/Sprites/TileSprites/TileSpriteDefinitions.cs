﻿/*
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

namespace Sovereign.ClientCore.Rendering.Sprites.TileSprites;

/// <summary>
///     Top-level record of tile sprite definitions.
/// </summary>
public sealed class TileSpriteDefinitions
{
    /// <summary>
    ///     Tile sprites.
    /// </summary>
    public List<TileSpriteRecord> TileSprites { get; set; } = new();

    /// <summary>
    ///     Serializable record of a tile sprite.
    /// </summary>
    public sealed class TileSpriteRecord
    {
        /// <summary>
        ///     Tile sprite ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Tile contexts.
        /// </summary>
        public List<TileContext> TileContexts { get; set; } = new();
    }
}