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
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
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
        Spritesheet
    }

    /// <summary>
    ///     Offset added to a list index to get the corresponding texture ID.
    /// </summary>
    private const int IndexOffset = 2;

    /// <summary>
    ///     Map from animated sprite ID to corresponding textures list entry.
    /// </summary>
    private readonly Dictionary<int, int> animatedSpriteIndices = new();

    private readonly AnimatedSpriteManager animatedSpriteManager;

    private readonly TextureAtlasManager atlasManager;
    private readonly AtlasMap atlasMap;
    private readonly ClientConfigurationManager clientConfigurationManager;

    /// <summary>
    ///     Map from spritesheet index to corresponding texture list entry.
    /// </summary>
    private readonly Dictionary<string, int> spritesheetIndices = new();

    private readonly SpriteSheetManager spriteSheetManager;

    /// <summary>
    ///     Registered textures for use in the GUI.
    /// </summary>
    private readonly List<TextureData> textures = new();

    public GuiTextureMapper(TextureAtlasManager atlasManager, SpriteSheetManager spriteSheetManager,
        AnimatedSpriteManager animatedSpriteManager, AtlasMap atlasMap,
        ClientConfigurationManager clientConfigurationManager)
    {
        this.atlasManager = atlasManager;
        this.spriteSheetManager = spriteSheetManager;
        this.animatedSpriteManager = animatedSpriteManager;
        this.atlasMap = atlasMap;
        this.clientConfigurationManager = clientConfigurationManager;

        animatedSpriteManager.OnAnimatedSpriteAdded += OnAnimatedSpriteAdded;
        animatedSpriteManager.OnAnimatedSpriteRemoved += OnAnimatedSpriteRemoved;
    }

    /// <summary>
    ///     Gets the texture data associated with the given texture index.
    /// </summary>
    /// <param name="textureId"></param>
    /// <returns></returns>
    public TextureData GetTextureDataForTextureId(IntPtr textureId)
    {
        return textures[(int)textureId - IndexOffset];
    }

    /// <summary>
    ///     Gets the ImGui texture ID for an animated sprite.
    /// </summary>
    /// <param name="animatedSpriteId">Animated sprite ID.</param>
    /// <returns>ImGui texture ID.</returns>
    public IntPtr GetTextureIdForAnimatedSprite(int animatedSpriteId)
    {
        if (!animatedSpriteIndices.TryGetValue(animatedSpriteId, out var index))
        {
            // For convenience, select dimensions to match the first south-facing sprite.
            var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
            var firstSprite = animatedSprite.GetSpriteForTime(0, Orientation.South);
            var spriteBounds = atlasMap.MapElements[firstSprite.Id];
            var clientConfiguration = clientConfigurationManager.ClientConfiguration;
            var width = spriteBounds.WidthInTiles * clientConfiguration.TileWidth;
            var height = spriteBounds.HeightInTiles * clientConfiguration.TileWidth;

            // Add record to table.
            index = textures.Count;
            textures.Add(new TextureData
            {
                SourceType = SourceType.AnimatedSprite,
                Id = animatedSpriteId,
                Width = width,
                Height = height
            });
            animatedSpriteIndices[animatedSpriteId] = index;
        }

        return new IntPtr(index + IndexOffset);
    }

    /// <summary>
    ///     Gets the ImGui texture ID for a full spritesheet.
    /// </summary>
    /// <param name="spritesheet">Spritesheet name.</param>
    /// <returns>ImGui texture ID.</returns>
    public IntPtr GetTextureIdForSpritesheet(string spritesheet)
    {
        if (!spritesheetIndices.TryGetValue(spritesheet, out var index))
        {
            // Find normalized coordiantes for spritesheet in the texture atlas.
            var atlas = atlasManager.TextureAtlas;
            if (atlas == null) throw new InvalidOperationException("Texture atlas is null.");
            var (topLeftAbsX, topLeftAbsY) = atlas.SpriteSheetMap[spritesheet];
            var sheetData = spriteSheetManager.SpriteSheets[spritesheet];
            var bottomRightAbsX = topLeftAbsX + sheetData.Surface.Properties.Width;
            var bottomRightAbsY = topLeftAbsY + sheetData.Surface.Properties.Height;

            // Add entry to table.
            index = textures.Count;
            textures.Add(new TextureData
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

        return new IntPtr(index + IndexOffset);
    }

    /// <summary>
    ///     Updates cached entries when a new animated sprite is created.
    /// </summary>
    /// <param name="animatedSpriteId">ID of new animated sprite.</param>
    private void OnAnimatedSpriteAdded(int animatedSpriteId)
    {
        // For new animated sprites, any sprites with id >= animatedSpriteId have their ID incremented.
        // Smaller IDs are unaffected.
        var affectedRows = new List<Tuple<int, int>>();

        // First pass, update the IDs in the texture records.
        foreach (var oldId in animatedSpriteIndices.Keys)
        {
            if (oldId < animatedSpriteId) continue;

            var newId = oldId + 1;

            var index = animatedSpriteIndices[oldId];
            textures[index].Id = newId;
            affectedRows.Add(Tuple.Create(oldId, index));
        }

        // Second pass, remove affected rows in the index cache.
        foreach (var oldRow in affectedRows)
            animatedSpriteIndices.Remove(oldRow.Item1);

        // Third pass, add new rows for affected IDs in the index cache.
        foreach (var oldRow in affectedRows)
        {
            var newId = oldRow.Item1 + 1;
            animatedSpriteIndices[newId] = oldRow.Item2;
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
        var affectedRows = new List<Tuple<int, int>>();

        // First pass, update the IDs in the records.
        foreach (var oldId in animatedSpriteIndices.Keys)
        {
            if (oldId <= animatedSpriteId) continue;

            var newId = oldId - 1;

            var index = animatedSpriteIndices[oldId];
            textures[index].Id = newId;
            affectedRows.Add(Tuple.Create(oldId, index));
        }

        // Second pass, remove the affected IDs.
        animatedSpriteIndices.Remove(animatedSpriteId);
        foreach (var oldRow in affectedRows)
        {
            animatedSpriteIndices.Remove(oldRow.Item1);
        }

        // Third pass, add new rows for affected IDs to the index cache.
        foreach (var oldRow in affectedRows)
        {
            var newId = oldRow.Item1 - 1;
            animatedSpriteIndices[newId] = oldRow.Item2;
        }
    }

    /// <summary>
    ///     Texture data for GUI rendering.
    /// </summary>
    public class TextureData
    {
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