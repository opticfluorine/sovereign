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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.EngineCore.World.Materials;

/// <summary>
///     Responsible for managing materials.
/// </summary>
public class MaterialManager
{
    /// <summary>
    ///     Filename for the material definitions file.
    /// </summary>
    public const string MaterialDefinitionsFilename = "MaterialDefinitions.json";

    private readonly IErrorHandler errorHandler;

    /// <summary>
    ///     Material definitions loader.
    /// </summary>
    private readonly MaterialDefinitionsLoader loader;

    private readonly ILogger logger;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder pathBuilder;

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
    ///     Material definitions.
    /// </summary>
    public List<Material> Materials { get; private set; } = new();

    /// <summary>
    ///     Initializes the materials.
    /// </summary>
    public void InitializeMaterials()
    {
        var definitions = LoadMaterialDefinitions();
        UnpackMaterialDefinitions(definitions);

        logger.Info("Loaded " + Materials.Count + " materials.");
    }

    /// <summary>
    ///     Loads the material definitions.
    /// </summary>
    /// <exception cref="FatalErrorException">
    ///     Thrown if the materials definitions file cannot be loaded.
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
    ///     Unpacks the material definitions.
    /// </summary>
    /// <param name="materialDefinitions"></param>
    private void UnpackMaterialDefinitions(MaterialDefinitions materialDefinitions)
    {
        /* Create a reserved material 0 for "air". */
        var airMat = new Material
        {
            Id = 0,
            MaterialName = "Air",
            MaterialSubtypes = new List<MaterialSubtype>
            {
                new()
                {
                    MaterialModifier = 0,
                    ObscuredTopFaceTileSpriteId = 0,
                    SideFaceTileSpriteId = 0,
                    TopFaceTileSpriteId = 0
                }
            }
        };

        /* Combine the loaded materials with the special type, then order and stash as a list. */
        Materials = materialDefinitions.Materials
            .Append(airMat)
            .OrderBy(material => material.Id)
            .ToList();
    }
}