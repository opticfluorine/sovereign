/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
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
    /// Provides additional client-side validation for materials.
    /// </summary>
    public sealed class RenderingMaterialValidator
    {

        private readonly TileSpriteManager tileSpriteManager;

        public RenderingMaterialValidator(TileSpriteManager tileSpriteManager)
        {
            this.tileSpriteManager = tileSpriteManager;
        }

        /// <summary>
        /// Validates the given list of materials.
        /// </summary>
        /// <param name="materials">Materials to validate.</param>
        /// <exception cref="RenderingMaterialException">
        /// Thrown if the materials are invalid.
        /// </exception>
        public void Validate(IList<Material> materials)
        {
            var sb = new StringBuilder();
            var valid = CheckTileSpriteIds(materials, sb);
            if (!valid)
                throw new RenderingMaterialException(sb.ToString().Trim());
        }

        /// <summary>
        /// Checks that all tile sprite references are valid.
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
                foreach (var badId in badIds)
                {
                    sb.Append("Material ").Append(badId).Append("\n");
                }
            }

            return valid;
        }

    }

}
