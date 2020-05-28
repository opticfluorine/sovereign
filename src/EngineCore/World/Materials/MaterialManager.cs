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
