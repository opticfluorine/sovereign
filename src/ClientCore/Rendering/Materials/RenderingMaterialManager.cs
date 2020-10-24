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
