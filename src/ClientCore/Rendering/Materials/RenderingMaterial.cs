/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.WorldLib.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Materials
{

    /// <summary>
    /// Client-side material for rendering.
    /// </summary>
    public sealed class RenderingMaterial
    {

        public TileSprite TopFaceTileSprite { get; private set; }

        public TileSprite SideFaceTileSprite { get; private set; }

        public TileSprite ObscuredTopFaceTileSprite { get; private set; }

        public RenderingMaterial(MaterialSubtype materialSubtype, 
            TileSpriteManager tileSpriteManager)
        {
            TopFaceTileSprite = tileSpriteManager
                .TileSprites[materialSubtype.TopFaceTileSpriteId];
            SideFaceTileSprite = tileSpriteManager
                .TileSprites[materialSubtype.SideFaceTileSpriteId];
            ObscuredTopFaceTileSprite = tileSpriteManager
                .TileSprites[materialSubtype.ObscuredTopFaceTileSpriteId];
        }

    }

}
