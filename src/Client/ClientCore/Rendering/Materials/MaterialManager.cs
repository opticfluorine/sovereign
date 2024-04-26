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
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Materials;

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
        ILogger logger, IErrorHandler errorHandler, TileSpriteManager tileSpriteManager)
    {
        /* Set dependencies. */
        this.loader = loader;
        this.pathBuilder = pathBuilder;
        this.logger = logger;
        this.errorHandler = errorHandler;

        tileSpriteManager.OnTileSpriteAdded += OnTileSpriteAdded;
        tileSpriteManager.OnTileSpriteRemoved += OnTileSpriteRemoved;
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
    ///     Inserts a new material with the given ID. Existing IDs are adjusted as necessary.
    /// </summary>
    /// <param name="id">ID of the new material.</param>
    public void InsertNew(int id)
    {
        if (id == Material.Air)
            throw new InvalidOperationException("Cannot insert before Air material.");

        Materials.Insert(id, new Material
        {
            Id = id,
            MaterialSubtypes = new List<MaterialSubtype> { new() }
        });
        for (var i = id + 1; i < Materials.Count; ++i)
        {
            Materials[i].Id++;
        }

        OnMaterialAdded?.Invoke(id);
    }

    /// <summary>
    ///     Updates an existing material in place.
    /// </summary>
    /// <param name="material">Updated material.</param>
    public void Update(Material material)
    {
        if (material.Id == Material.Air)
            throw new InvalidOperationException("Cannot modify Air material.");

        Materials[material.Id] = new Material(material);
        OnMaterialUpdated?.Invoke(material.Id);
    }

    /// <summary>
    ///     Removes an existing material. Existing IDs are adjusted as necessary.
    /// </summary>
    /// <param name="id">ID of the material to remove.</param>
    public void Remove(int id)
    {
        if (id == Material.Air)
            throw new InvalidOperationException("Cannot remove Air material.");

        Materials.RemoveAt(id);
        for (var i = id; i < Materials.Count; ++i)
        {
            Materials[i].Id--;
        }

        OnMaterialRemoved?.Invoke(id);
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

    /// <summary>
    ///     Saves the current materials to the materials definition file.
    /// </summary>
    private void SaveDefinitions()
    {
        // Build definitions.
        var defs = new MaterialDefinitions
        {
            Materials = Materials.Slice(1, Materials.Count - 1)
        };

        // Save.
        loader.SaveDefinitions(MaterialDefinitionsFilename, defs);
    }

    /// <summary>
    ///     Called when a new tile sprite is added.
    /// </summary>
    /// <param name="tileSpriteId">Added tile sprite ID.</param>
    private void OnTileSpriteAdded(int tileSpriteId)
    {
        foreach (var material in Materials)
        foreach (var subtype in material.MaterialSubtypes)
        {
            if (subtype.SideFaceTileSpriteId >= tileSpriteId) subtype.SideFaceTileSpriteId++;
            if (subtype.TopFaceTileSpriteId >= tileSpriteId) subtype.TopFaceTileSpriteId++;
            if (subtype.ObscuredTopFaceTileSpriteId >= tileSpriteId) subtype.ObscuredTopFaceTileSpriteId++;
        }

        SaveDefinitions();
    }

    /// <summary>
    ///     Called when a tile sprite is removed.
    /// </summary>
    /// <param name="tileSpriteId">Removed tile sprite ID.</param>
    private void OnTileSpriteRemoved(int tileSpriteId)
    {
        foreach (var material in Materials)
        foreach (var subtype in material.MaterialSubtypes)
        {
            if (subtype.SideFaceTileSpriteId > tileSpriteId) subtype.SideFaceTileSpriteId--;
            if (subtype.TopFaceTileSpriteId > tileSpriteId) subtype.TopFaceTileSpriteId--;
            if (subtype.ObscuredTopFaceTileSpriteId > tileSpriteId) subtype.ObscuredTopFaceTileSpriteId--;
        }

        SaveDefinitions();
    }

    /// <summary>
    ///     Event triggered when a material is added.
    ///     Parameter is the added material ID.
    /// </summary>
    /// <remarks>
    ///     Since the materials are maintained as a sequential list, any materials with
    ///     IDs greater than or equal to the new material's ID are incremented by one.
    /// </remarks>
    public event Action<int>? OnMaterialAdded;

    /// <summary>
    ///     Event triggered when a material is updated.
    ///     Parameter is the updated material ID.
    /// </summary>
    public event Action<int>? OnMaterialUpdated;

    /// <summary>
    ///     Event triggered when a material is removed.
    ///     Parameter is the removed material ID.
    /// </summary>
    /// <remarks>
    ///     Since the materials are maintained as a sequential list, any materials with
    ///     IDs greater than the new material's ID are decremented by one.
    /// </remarks>
    public event Action<int>? OnMaterialRemoved;
}