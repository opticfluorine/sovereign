using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;
using Sovereign.WorldLib.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineCore.World.Material
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
        public MaterialDefinitions MaterialDefinitions { get; private set; }

        public MaterialManager(MaterialDefinitionsLoader loader, IResourcePathBuilder pathBuilder,
            ILogger logger, IErrorHandler errorHandler)
        {
            /* Set dependencies. */
            this.loader = loader;
            this.pathBuilder = pathBuilder;
            this.logger = logger;
            this.errorHandler = errorHandler;

            /* Load the material definitions. */
            MaterialDefinitions = LoadMaterialDefinitions();
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

    }

}
