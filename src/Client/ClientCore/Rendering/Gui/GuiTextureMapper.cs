// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Rendering.Sprites.TileSprites;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Gui;

/// <summary>
///     Maps ImGui texture IDs to more detailed texture identifiers internally.
/// </summary>
public class GuiTextureMapper
{
    /// <summary>
    ///     Supported source data types.
    /// </summary>
    public enum SourceType
    {
        /// <summary>
        ///     Texture is an animated sprite.
        /// </summary>
        AnimatedSprite,

        /// <summary>
        ///     Texture is a full spritesheet.
        /// </summary>
        Spritesheet,

        /// <summary>
        ///     Texture is a static sprite.
        /// </summary>
        Sprite,

        /// <summary>
        ///     Texture is multiple overlapping sources.
        /// </summary>
        Multiple,

        /// <summary>
        ///     Texture is a custom animated sprite not in the animated sprite table.
        /// </summary>
        CustomAnimatedSprite
    }

    /// <summary>
    ///     Offset added to a list index to get the corresponding texture ID.
    /// </summary>
    private const int IndexOffset = 2;

    /// <summary>
    ///     Map from animated sprite ID to corresponding textures list entry.
    /// </summary>
    private readonly Dictionary<Tuple<int, Orientation, AnimationPhase>, int> animatedSpriteIndices = new();

    private readonly AnimatedSpriteManager animatedSpriteManager;

    private readonly TextureAtlasManager atlasManager;
    private readonly AtlasMap atlasMap;

    /// <summary>
    ///     Map from custom ID to corresponding texture list entry.
    /// </summary>
    private readonly Dictionary<string, int> customIndices = new();

    /// <summary>
    ///     Queue of indices that can be reclaimed for use.
    /// </summary>
    private readonly Queue<int> reclaimableIndices = new();

    private readonly RendererOptions rendererOptions;

    /// <summary>
    ///     Map from sprite ID to corresponding textures list entry.
    /// </summary>
    private readonly Dictionary<int, int> spriteIndices = new();

    /// <summary>
    ///     Map from spritesheet index to corresponding texture list entry.
    /// </summary>
    private readonly Dictionary<string, int> spritesheetIndices = new();

    private readonly SpriteSheetManager spriteSheetManager;

    /// <summary>
    ///     Registered textures for use in the GUI.
    /// </summary>
    private readonly List<TextureData> textures = new();

    /// <summary>
    ///     Map from tilesprite ID (and neighbors) to corresponding texture list entry (front face).
    /// </summary>
    private readonly Dictionary<Tuple<int, TileContextKey>, int> tileSpriteIndices = new();

    private readonly TileSpriteManager tileSpriteManager;

    public GuiTextureMapper(TextureAtlasManager atlasManager, SpriteSheetManager spriteSheetManager,
        AnimatedSpriteManager animatedSpriteManager, AtlasMap atlasMap,
        TileSpriteManager tileSpriteManager, IOptions<RendererOptions> rendererOptions)
    {
        this.atlasManager = atlasManager;
        this.spriteSheetManager = spriteSheetManager;
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.tileSpriteManager = tileSpriteManager;
        this.rendererOptions = rendererOptions.Value;

        animatedSpriteManager.OnAnimatedSpriteAdded += OnAnimatedSpriteAdded;
        animatedSpriteManager.OnAnimatedSpriteRemoved += OnAnimatedSpriteRemoved;
        tileSpriteManager.OnTileSpriteAdded += OnTileSpriteChange;
        tileSpriteManager.OnTileSpriteRemoved += OnTileSpriteChange;
        tileSpriteManager.OnTileSpriteUpdated += OnTileSpriteChange;
    }

    /// <summary>
    ///     Gets the texture data associated with the given texture index.
    /// </summary>
    /// <param name="textureId"></param>
    /// <returns></returns>
    public TextureData GetTextureDataForTextureId(ImTextureID textureId)
    {
        return textures[(int)textureId.Handle - IndexOffset];
    }

    /// <summary>
    ///     Gets the ImGui texture ID for an animated sprite.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    /// <returns>ImGui texture ID.</returns>
    public ImTextureID GetTextureIdForAnimatedSprite(int animatedSpriteId, Orientation orientation,
        AnimationPhase phase)
    {
        var key = Tuple.Create(animatedSpriteId, orientation, phase);
        if (!animatedSpriteIndices.TryGetValue(key, out var index))
        {
            // For convenience, select dimensions to match the first frame in the animation.
            var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
            var firstSprite = animatedSprite.GetPhaseData(phase).GetSpriteForTime(0, orientation);
            var spriteBounds = atlasMap.MapElements[firstSprite.Id];
            var width = spriteBounds.WidthInTiles * rendererOptions.TileWidth;
            var height = spriteBounds.HeightInTiles * rendererOptions.TileWidth;

            // Add record to table.
            index = AddTextureData(new TextureData
            {
                SourceType = SourceType.AnimatedSprite,
                Id = animatedSpriteId,
                Orientation = orientation,
                AnimationPhase = phase,
                Width = width,
                Height = height
            });
            animatedSpriteIndices[key] = index;
        }

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a custom animated sprite that is not in the animated sprite table.
    /// </summary>
    /// <param name="customSpriteId">Unique identifier for the custom animated sprite.</param>
    /// <param name="customSprite">Custom animated sprite.</param>
    /// <param name="orientation">Orientation.</param>
    /// <param name="phase">Animation phase.</param>
    /// <returns>ImGui texture ID.</returns>
    public ImTextureID GetTextureIdForCustomAnimatedSprite(string customSpriteId, AnimatedSprite customSprite,
        Orientation orientation, AnimationPhase phase)
    {
        if (!customIndices.TryGetValue(customSpriteId, out var index))
        {
            index = AddTextureData(new TextureData
            {
                SourceType = SourceType.CustomAnimatedSprite
            });
            customIndices[customSpriteId] = index;
        }

        // Update existing record in case any paramters were changed since the last call.
        var firstSprite = customSprite.GetPhaseData(phase).GetSpriteForTime(0, orientation);
        var spriteBounds = atlasMap.MapElements[firstSprite.Id];
        var width = spriteBounds.WidthInTiles * rendererOptions.TileWidth;
        var height = spriteBounds.HeightInTiles * rendererOptions.TileWidth;

        var texData = textures[index];
        texData.CustomAnimatedSprite = customSprite;
        texData.Orientation = orientation;
        texData.AnimationPhase = phase;
        texData.Width = width;
        texData.Height = height;

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a full spritesheet.
    /// </summary>
    /// <param name="spritesheet">Spritesheet name.</param>
    /// <returns>ImGui texture ID.</returns>
    public ImTextureID GetTextureIdForSpritesheet(string spritesheet)
    {
        if (!spritesheetIndices.TryGetValue(spritesheet, out var index))
        {
            // Find normalized coordinates for spritesheet in the texture atlas.
            var atlas = atlasManager.TextureAtlas;
            if (atlas == null) throw new InvalidOperationException("Texture atlas is null.");
            var (topLeftAbsX, topLeftAbsY) = atlas.SpriteSheetMap[spritesheet];
            var sheetData = spriteSheetManager.SpriteSheets[spritesheet];
            var bottomRightAbsX = topLeftAbsX + sheetData.Surface.Properties.Width;
            var bottomRightAbsY = topLeftAbsY + sheetData.Surface.Properties.Height;

            // Add entry to table.
            index = AddTextureData(new TextureData
            {
                SourceType = SourceType.Spritesheet,
                StartX = (float)topLeftAbsX / atlas.Width,
                StartY = (float)topLeftAbsY / atlas.Height,
                EndX = (float)bottomRightAbsX / atlas.Width,
                EndY = (float)bottomRightAbsY / atlas.Height,
                Width = sheetData.Surface.Properties.Width,
                Height = sheetData.Surface.Properties.Height
            });
            spritesheetIndices[spritesheet] = index;
        }

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a static sprite.
    /// </summary>
    /// <param name="spriteId">Sprite ID.</param>
    /// <returns>ImGui texture ID.</returns>
    public ImTextureID GetTextureIdForSprite(int spriteId)
    {
        if (!spriteIndices.TryGetValue(spriteId, out var index))
        {
            if (spriteId >= atlasMap.MapElements.Count)
                throw new IndexOutOfRangeException($"Sprite {spriteId} does not exist.");

            // Add entry to table.
            var spriteData = atlasMap.MapElements[spriteId];
            index = AddTextureData(new TextureData
            {
                SourceType = SourceType.Sprite,
                StartX = spriteData.NormalizedLeftX,
                StartY = spriteData.NormalizedTopY,
                EndX = spriteData.NormalizedRightX,
                EndY = spriteData.NormalizedBottomY,
                Width = spriteData.WidthInTiles * rendererOptions.TileWidth,
                Height = spriteData.HeightInTiles * rendererOptions.TileWidth
            });
            spriteIndices[spriteId] = index;
        }

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a tile sprite in a specific context.
    /// </summary>
    /// <param name="tileSpriteId">Tile sprite ID.</param>
    /// <param name="contextKey">Tile context key.</param>
    /// <returns>ImGui texture ID.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the tile sprite ID is out of range.</exception>
    public ImTextureID GetTextureIdForTileSprite(int tileSpriteId, TileContextKey contextKey)
    {
        if (!tileSpriteIndices.TryGetValue(Tuple.Create(tileSpriteId, contextKey), out var index))
        {
            if (tileSpriteId >= tileSpriteManager.TileSprites.Count)
                throw new IndexOutOfRangeException($"Tile sprite {tileSpriteId} does not exist.");

            // Resolve tile sprite through its context to a list of animated sprite layers.
            var layers = tileSpriteManager.TileSprites[tileSpriteId].GetMatchingAnimatedSpriteIds(contextKey);
            if (layers.Count == 0)
                throw new IndexOutOfRangeException($"Tile sprite {tileSpriteId} contains no layers.");

            // Assume constant sprite size through all layers. Use first layer as source.
            var firstLayerTexId = GetTextureIdForAnimatedSprite(layers[0], Orientation.South, AnimationPhase.Default);
            var firstLayerTexData = GetTextureDataForTextureId(firstLayerTexId);

            // Create texture record.
            index = AddTextureData(new TextureData
            {
                SourceType = SourceType.Multiple,
                Layers = layers
                    .Select(id => GetTextureIdForAnimatedSprite(id, Orientation.South, AnimationPhase.Default))
                    .ToList(),
                Width = firstLayerTexData.Width,
                Height = firstLayerTexData.Height
            });
            tileSpriteIndices[Tuple.Create(tileSpriteId, contextKey)] = index;
        }

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a custom tile sprite in a specific context.
    /// </summary>
    /// <param name="customId">Unique identifier for the custom sprite.</param>
    /// <param name="customSprite">Custom tile sprite.</param>
    /// <param name="contextKey">Tile sprite context key.</param>
    /// <returns>ImGui texture ID.</returns>
    public ImTextureID GetTextureIdForCustomTileSprite(string customId, TileSprite customSprite,
        TileContextKey contextKey)
    {
        if (!customIndices.TryGetValue(customId, out var index))
        {
            index = AddTextureData(new TextureData());
            customIndices[customId] = index;
        }

        // Update entire record in case there were any changes.
        var texData = textures[index];
        texData.SourceType = SourceType.Multiple;
        texData.Layers ??= new List<ImTextureID>();
        texData.Layers.Clear();
        texData.Layers.AddRange(
            customSprite
                .GetMatchingAnimatedSpriteIds(contextKey)
                .Select(id => GetTextureIdForAnimatedSprite(id, Orientation.South, AnimationPhase.Default)));
        var layerTexData = textures[(int)texData.Layers[0].Handle - IndexOffset];
        texData.Width = layerTexData.Width;
        texData.Height = layerTexData.Height;

        return new ImTextureID(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the next available texture ID.
    /// </summary>
    /// <returns>Next available texture ID.</returns>
    private int GetNextTextureId()
    {
        return reclaimableIndices.TryDequeue(out var next) ? next : textures.Count;
    }

    /// <summary>
    ///     Adds the given texture data to the next available position in the list.
    /// </summary>
    /// <param name="textureData">Texture data.</param>
    /// <returns>Index into texture data list where the data resides.</returns>
    private int AddTextureData(TextureData textureData)
    {
        var index = GetNextTextureId();
        if (index >= textures.Count)
            textures.Insert(index, textureData);
        else
            textures[index] = textureData;

        return index;
    }

    /// <summary>
    ///     Updates cached entries when a new animated sprite is created.
    /// </summary>
    /// <param name="animatedSpriteId">ID of new animated sprite.</param>
    private void OnAnimatedSpriteAdded(int animatedSpriteId)
    {
        // For new animated sprites, any sprites with id >= animatedSpriteId have their ID incremented.
        // Smaller IDs are unaffected.
        var affectedRows = new List<Tuple<Tuple<int, Orientation, AnimationPhase>, int>>();

        // First pass, update the IDs in the texture records.
        foreach (var key in animatedSpriteIndices.Keys)
        {
            var oldId = key.Item1;
            if (oldId < animatedSpriteId) continue;

            var newId = oldId + 1;

            var index = animatedSpriteIndices[key];
            textures[index].Id = newId;
            affectedRows.Add(Tuple.Create(key, index));
        }

        // Second pass, remove affected rows in the index cache.
        foreach (var oldRow in affectedRows)
            animatedSpriteIndices.Remove(oldRow.Item1);

        // Third pass, add new rows for affected IDs in the index cache.
        foreach (var oldRow in affectedRows)
        {
            var newKey = Tuple.Create(oldRow.Item1.Item1 + 1, oldRow.Item1.Item2, oldRow.Item1.Item3);
            animatedSpriteIndices[newKey] = oldRow.Item2;
        }
    }

    /// <summary>
    ///     Updates cached entries when a new animated sprite is removed.
    /// </summary>
    /// <param name="animatedSpriteId">ID of removed animated sprite.</param>
    private void OnAnimatedSpriteRemoved(int animatedSpriteId)
    {
        // For removed animated sprites, any sprites with id > animatedSpriteId will be decremented.
        // Smaller IDs are not affected.
        // We don't remove the corresponding texture record to avoid a reordering of higher texture IDs.
        var affectedRows = new List<Tuple<Tuple<int, Orientation, AnimationPhase>, int>>();
        var rowsToRemove = new List<Tuple<int, Orientation, AnimationPhase>>();

        // First pass, update the IDs in the records.
        foreach (var key in animatedSpriteIndices.Keys)
        {
            var oldId = key.Item1;
            if (oldId < animatedSpriteId) continue;
            if (oldId == animatedSpriteId)
            {
                rowsToRemove.Add(key);
                continue;
            }

            var newId = oldId - 1;

            var index = animatedSpriteIndices[key];
            textures[index].Id = newId;
            affectedRows.Add(Tuple.Create(key, index));
        }

        // Second pass, remove the affected IDs.
        foreach (var key in rowsToRemove)
        {
            reclaimableIndices.Enqueue(animatedSpriteIndices[key]);
            animatedSpriteIndices.Remove(key);
        }

        foreach (var oldRow in affectedRows) animatedSpriteIndices.Remove(oldRow.Item1);

        // Third pass, add new rows for affected IDs to the index cache.
        foreach (var oldRow in affectedRows)
        {
            var newKey = Tuple.Create(oldRow.Item1.Item1 - 1, oldRow.Item1.Item2, oldRow.Item1.Item3);
            animatedSpriteIndices[newKey] = oldRow.Item2;
        }
    }

    /// <summary>
    ///     Called when the tile sprites are changed (added, updated, removed).
    /// </summary>
    /// <param name="tileSpriteId">Tile sprite ID.</param>
    public void OnTileSpriteChange(int tileSpriteId)
    {
        // Invalidate all tile sprite texture caches, flagging the texture indices to be reclaimed.
        foreach (var texId in tileSpriteIndices.Values)
        {
            reclaimableIndices.Enqueue(texId);
        }

        tileSpriteIndices.Clear();
    }

    /// <summary>
    ///     Texture data for GUI rendering.
    /// </summary>
    public class TextureData
    {
        /// <summary>
        ///     For SourceType "AnimatedSprite" and "CustomAnimatedSprite", animation phase.
        /// </summary>
        public AnimationPhase AnimationPhase;

        /// <summary>
        ///     For SourceType "CustomAnimatedSprite", the custom animated sprite to be drawn.
        /// </summary>
        public AnimatedSprite? CustomAnimatedSprite;

        /// <summary>
        ///     For coordinate-based source types, the normalized top Y coordinate.
        /// </summary>
        public float EndX;

        /// <summary>
        ///     For coordinate-based source types, the normalized bottom Y coordinate.
        /// </summary>
        public float EndY;

        /// <summary>
        ///     Height in pixels.
        /// </summary>
        public float Height;

        /// <summary>
        ///     For ID-based source types, the ID of the underlying resource.
        /// </summary>
        public int Id;

        /// <summary>
        ///     For SourceType "Multiple", the layers to be drawn.
        /// </summary>
        public List<ImTextureID>? Layers;

        /// <summary>
        ///     For SourceType "AnimatedSprite" and "CustomAnimatedSprite", orientation.
        /// </summary>
        public Orientation Orientation;

        /// <summary>
        ///     Source data type.
        /// </summary>
        public SourceType SourceType;

        /// <summary>
        ///     For coordinate-based source types, the normalized left X coordinate.
        /// </summary>
        public float StartX;

        /// <summary>
        ///     For coordinate-based source types, the normalized right X coordinate.
        /// </summary>
        public float StartY;

        /// <summary>
        ///     Width in pixels.
        /// </summary>
        public float Width;
    }
}