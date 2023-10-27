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
using System.Linq;
using System.Text;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.World.Materials;

namespace Sovereign.ClientCore.Rendering.Materials;

/// <summary>
///     Provides additional client-side validation for materials.
/// </summary>
public sealed class RenderingMaterialValidator
{
    private readonly TileSpriteManager tileSpriteManager;

    public RenderingMaterialValidator(TileSpriteManager tileSpriteManager)
    {
        this.tileSpriteManager = tileSpriteManager;
    }

    /// <summary>
    ///     Validates the given list of materials.
    /// </summary>
    /// <param name="materials">Materials to validate.</param>
    /// <exception cref="RenderingMaterialException">
    ///     Thrown if the materials are invalid.
    /// </exception>
    public void Validate(IList<Material> materials)
    {
        var sb = new StringBuilder();
        var valid = CheckTileSpriteIds(materials, sb);
        if (!valid)
            throw new RenderingMaterialException(sb.ToString().Trim());
    }

    /// <summary>
    ///     Checks that all tile sprite references are valid.
    /// </summary>
    /// <param name="materials">Materials to check.</param>
    /// <param name="sb">StringBuilder for error reporting.</param>
    /// <returns>true if valid, false otherwise.</returns>
    private bool CheckTileSpriteIds(IList<Material> materials, StringBuilder sb)
    {
        var limit = tileSpriteManager.TileSprites.Count;
        var badIds = materials
            .Where(material => material.MaterialSubtypes
                .Where(subtype => subtype.TopFaceTileSpriteId >= limit
                                  || subtype.SideFaceTileSpriteId >= limit
                                  || subtype.ObscuredTopFaceTileSpriteId >= limit)
                .Count() > 0)
            .Select(material => material.Id);
        var valid = badIds.Count() == 0;

        if (!valid)
        {
            sb.Append("The following materials reference unknown Tile Sprites:\n\n");
            foreach (var badId in badIds) sb.Append("Material ").Append(badId).Append("\n");
        }

        return valid;
    }
}