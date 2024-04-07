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
    ///     Inserts an empty animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if id less than 0 or greater than AnimatedSprites.Count.</exception>
    public void InsertNew(int id)
    {
        if (id < 0 || id > AnimatedSprites.Count)
            throw new IndexOutOfRangeException("Bad list index.");

        var newSprite = new AnimatedSprite();
        newSprite.Phases[AnimationPhase.Default].Frames[Orientation.South] =
            new List<Sprite> { spriteManager.Sprites[0] };
        AnimatedSprites.Insert(id, newSprite);
        SaveDefinitions();
        OnAnimatedSpriteAdded?.Invoke(id);
    }

    /// <summary>
    ///     Updates an existing animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <param name="newValue">New animated sprite data.</param>
    /// <remarks>
    ///     The new value is copied into the animated sprite table.
    /// </remarks>
    public void Update(int id, AnimatedSprite newValue)
    {
        if (id < 0 || id >= AnimatedSprites.Count)
            throw new IndexOutOfRangeException("Bad list index.");

        AnimatedSprites[id] = new AnimatedSprite(newValue);
        SaveDefinitions();
    }

    /// <summary>
    ///     Removes an existing animated sprite at the given position.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <remarks>
    ///     This method does not check for downstream resource dependencies on the
    ///     deleted animated sprite. Ensure that any downstream dependencies are resolved
    ///     before deleted an animated sprite.
    /// </remarks>
    public void Remove(int id)
    {
        if (id < 0 || id >= AnimatedSprites.Count)
            throw new IndexOutOfRangeException("Bad list index.");

        AnimatedSprites.RemoveAt(id);
        SaveDefinitions();
        OnAnimatedSpriteRemoved?.Invoke(id);
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
                Phases = animSprite.Phases.Select(
                    p => Tuple.Create(p.Key,
                        new AnimatedSpriteDefinitions.AnimatedSpritePhaseDefinition
                        {
                            AnimationTimestep = p.Value.FrameTime,
                            Faces = p.Value.Frames
                                .Select(f => Tuple.Create(f.Key,
                                    new AnimatedSpriteDefinitions.AnimatedSpriteFaceDefinition
                                    {
                                        SpriteIds = f.Value.Select(sprite => sprite.Id).ToList()
                                    }))
                                .ToDictionary(t => t.Item1, t => t.Item2)
                        })).ToDictionary(t => t.Item1, t => t.Item2)
            };
            defs.AnimatedSprites.Add(def);
        }

        // Save.
        try
        {
            var path = pathBuilder.BuildPathToResource(ResourceType.Sprite, DefinitionsFilename);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, defs);
            Logger.InfoFormat("Saved {0} animated sprites.", defs.AnimatedSprites.Count);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to save animated sprite definitions.", e);
        }
    }

    /// <summary>
    ///     Event triggered when an animated sprite is added.
    ///     Parameter is the added sprite ID.
    /// </summary>
    /// <remarks>
    ///     Since the animated sprites are maintained as a sequential list, any animated sprites with
    ///     IDs greater than or equal to the new sprite's ID are incremented by one.
    /// </remarks>
    public event Action<int>? OnAnimatedSpriteAdded;

    /// <summary>
    ///     Event triggered when an animated sprite is removed.
    ///     Parameter is the removed sprite ID.
    /// </summary>
    /// <remarks>
    ///     Since the animated sprites are maintained as a sequential list, any animated sprites with IDs
    ///     greater than the removed sprite's ID are decremented by one.
    /// </remarks>
    public event Action<int>? OnAnimatedSpriteRemoved;
}