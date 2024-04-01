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
using System.IO;
using System.Linq;
using System.Text.Json;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Resources;

namespace Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;

/// <summary>
///     Manages the animated sprites.
/// </summary>
public sealed class AnimatedSpriteManager
{
    /// <summary>
    ///     Resource category used by the animated sprite definitions.
    /// </summary>
    public const ResourceType DefinitionsResourceType = ResourceType.Sprite;

    /// <summary>
    ///     Animated sprite definitions filename.
    /// </summary>
    public const string DefinitionsFilename = "AnimatedSpriteDefinitions.json";

    /// <summary>
    ///     Animated sprites.
    /// </summary>
    public readonly List<AnimatedSprite> AnimatedSprites = new();

    /// <summary>
    ///     Animated sprite definitions loader.
    /// </summary>
    private readonly AnimatedSpriteDefinitionsLoader loader;

    /// <summary>
    ///     Resource path builder.
    /// </summary>
    private readonly IResourcePathBuilder pathBuilder;

    /// <summary>
    ///     Sprite manager.
    /// </summary>
    private readonly SpriteManager spriteManager;

    public AnimatedSpriteManager(AnimatedSpriteDefinitionsLoader loader,
        IResourcePathBuilder pathBuilder, SpriteManager spriteManager)
    {
        this.loader = loader;
        this.pathBuilder = pathBuilder;
        this.spriteManager = spriteManager;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    public IErrorHandler ErrorHandler { private get; set; } = NullErrorHandler.Instance;


    /// <summary>
    ///     Initializes the animated sprites.
    /// </summary>
    public void InitializeAnimatedSprites()
    {
        var definitions = LoadDefinitions();
        UnpackDefinitions(definitions);

        Logger.Info("Loaded " + AnimatedSprites.Count + " animated sprites.");
    }

    /// <summary>
    ///     Updates the animated sprite table and saves the definitions to the file.
    /// </summary>
    public void UpdateAndSave()
    {
        // Notify downstream resources of the updates.
        OnAnimatedSpritesChanged?.Invoke();
    }

    /// <summary>
    ///     Inserts an empty animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if id less than 0 or greater than AnimatedSprites.Count.</exception>
    public void InsertNew(int id)
    {
        if (id < 0 || id > AnimatedSprites.Count)
            throw new IndexOutOfRangeException("Bad list index.");

        AnimatedSprites.Insert(id, new AnimatedSprite());
    }

    /// <summary>
    ///     Updates an existing animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <param name="newValue">New animated sprite data.</param>
    public void Update(int id, AnimatedSprite newValue)
    {
    }

    /// <summary>
    ///     Deletes an existing animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    public void Delete(int id)
    {
    }

    /// <summary>
    ///     Loads the animated sprite definitions.
    /// </summary>
    /// <returns>Animated sprite definitions.</returns>
    /// <exception cref="FatalErrorException">
    ///     Thrown if the definitions cannot be loaded.
    /// </exception>
    private AnimatedSpriteDefinitions LoadDefinitions()
    {
        var filename = pathBuilder.BuildPathToResource(DefinitionsResourceType,
            DefinitionsFilename);
        try
        {
            return loader.LoadDefinitions(filename);
        }
        catch (Exception e)
        {
            /* Log and throw a fatal error. */
            Logger.Fatal("Failed to load the animated sprite definitions.", e);
            ErrorHandler.Error("Failed to load the animated sprite definitions.\n"
                               + "Refer to the error log for details.");

            throw new FatalErrorException();
        }
    }

    /// <summary>
    ///     Unpacks the animated sprite definitions.
    /// </summary>
    /// <param name="definitions">Definitions to unpack.</param>
    private void UnpackDefinitions(AnimatedSpriteDefinitions definitions)
    {
        foreach (var def in definitions.AnimatedSprites.OrderBy(def => def.Id))
            AnimatedSprites.Add(new AnimatedSprite(def, spriteManager));
    }

    /// <summary>
    ///     Saves the latest animated sprite definitions to the file.
    /// </summary>
    private void SaveDefinitions()
    {
        // Generate a new set of definitions from the list of animated sprites.
        var defs = new AnimatedSpriteDefinitions();
        for (var i = 0; i < AnimatedSprites.Count; ++i)
        {
            var animSprite = AnimatedSprites[i];
            var def = new AnimatedSpriteDefinitions.AnimatedSpriteDefinition
            {
                Id = i,
                AnimationTimestep = animSprite.FrameTime,
                Faces = animSprite.Faces.Select(
                        face => new Tuple<Orientation, AnimatedSpriteDefinitions.AnimatedSpriteFaceDefinition>(face.Key,
                            new AnimatedSpriteDefinitions.AnimatedSpriteFaceDefinition
                                { SpriteIds = face.Value.Select(sprite => sprite.Id).ToList() }))
                    .ToDictionary(t => t.Item1, t => t.Item2)
            };
            defs.AnimatedSprites.Add(def);
        }

        // Save.
        try
        {
            using var stream = new FileStream(DefinitionsFilename, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, defs);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to save animated sprite definitions.", e);
        }
    }

    /// <summary>
    ///     Event triggered when the animated sprites are updated.
    /// </summary>
    public event Action? OnAnimatedSpritesChanged;
}