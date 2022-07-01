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
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using Sovereign.WorldLib.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.World.Materials
{

    /// <summary>
    /// Responsible for managing materials.
    /// </summary>
    public class MaterialManager
    {

        /// <summary>
        /// Filename for the material definitions file.
        /// </summary>
        public const string MaterialDefinitionsFilename = "MaterialDefinitions.yaml";

        private ILogger logger;

        private IErrorHandler errorHandler;

        /// <summary>
        /// Material definitions loader.
        /// </summary>
        private readonly MaterialDefinitionsLoader loader;

        /// <summary>
        /// Resource path builder.
        /// </summary>
        private readonly IResourcePathBuilder pathBuilder;

        /// <summary>
        /// Material definitions.
        /// </summary>
        public IList<Material> Materials { get; private set; }

        public MaterialManager(MaterialDefinitionsLoader loader, IResourcePathBuilder pathBuilder,
            ILogger logger, IErrorHandler errorHandler)
        {
            /* Set dependencies. */
            this.loader = loader;
            this.pathBuilder = pathBuilder;
            this.logger = logger;
            this.errorHandler = errorHandler;
        }

        /// <summary>
        /// Initializes the materials.
        /// </summary>
        public void InitializeMaterials()
        {
            var definitions = LoadMaterialDefinitions();
            UnpackMaterialDefinitions(definitions);

            logger.Info("Loaded " + Materials.Count + " materials.");
        }

        /// <summary>
        /// Loads the material definitions.
        /// </summary>
        /// <exception cref="FatalErrorException">
        /// Thrown if the materials definitions file cannot be loaded.
        /// </exception>
        private MaterialDefinitions LoadMaterialDefinitions()
        {
            /* Get the filename. */
            var filename = pathBuilder.BuildPathToResource(ResourceType.World,
                MaterialDefinitionsFilename);

            /* Attempt to load the material definitions file. */
            try
            {
                return loader.LoadDefinitions(filename);
            }
            catch (Exception e)
            {
                /* Failed to load the material definitions. */
                var msg = "Failed to load the material definitions.";
                logger.Fatal(msg, e);
                errorHandler.Error(msg);

                /* Signal the fatal error. */
                throw new FatalErrorException();
            }
        }

        /// <summary>
        /// Unpacks the material definitions.
        /// </summary>
        /// <param name="materialDefinitions"></param>
        private void UnpackMaterialDefinitions(MaterialDefinitions materialDefinitions)
        {
            Materials = materialDefinitions.Materials
                .OrderBy(material => material.Id)
                .ToList();
        }

    }

}
