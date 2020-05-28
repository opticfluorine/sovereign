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

using Castle.Core.Logging;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.World.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Materials
{

    /// <summary>
    /// Manages material mappings for client-side rendering.
    /// </summary>
    public sealed class RenderingMaterialManager
    {

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;

        private readonly MaterialManager materialManager;

        private readonly TileSpriteManager tileSpriteManager;

        private readonly RenderingMaterialValidator validator;

        /// <summary>
        /// Rendering materials mapped by tuple (material ID, subtype ID).
        /// </summary>
        public IDictionary<Tuple<int, int>, RenderingMaterial> RenderingMaterials { get; private set; }
            = new Dictionary<Tuple<int, int>, RenderingMaterial>();

        public RenderingMaterialManager(MaterialManager materialManager,
            TileSpriteManager tileSpriteManager, RenderingMaterialValidator validator)
        {
            this.materialManager = materialManager;
            this.tileSpriteManager = tileSpriteManager;
            this.validator = validator;
        }

        /// <summary>
        /// Initializes rendering materials.
        /// </summary>
        /// 
        /// This must be called after the materials have been initialized.
        public void InitializeRenderingMaterials()
        {
            ValidateMaterials();
            UnpackMaterials();

            Logger.Info("Unpacked " + RenderingMaterials.Count + " rendering materials.");
        }

        /// <summary>
        /// Performs additional client-side validation of materials.
        /// </summary>
        private void ValidateMaterials()
        {
            try
            {
                validator.Validate(materialManager.Materials);
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to unpack materials for rendering.", e);
                ErrorHandler.Error("Failed to unpack materials for rendering.\n"
                    + "Refer to the error log for details.");
                throw new FatalErrorException();
            }
        }

        /// <summary>
        /// Unpacks the materials into rendering materials.
        /// </summary>
        private void UnpackMaterials()
        {
            foreach (var material in materialManager.Materials)
            {
                foreach (var subtype in material.MaterialSubtypes)
                {
                    var key = new Tuple<int, int>(material.Id, subtype.MaterialModifier);
                    RenderingMaterials[key] = new RenderingMaterial(subtype, tileSpriteManager);
                }
            }
        }

    }

}
