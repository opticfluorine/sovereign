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

using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Groups drawables in range by their layer.
/// </summary>
public sealed class WorldLayerGrouper
{
    /// <summary>
    ///     Reusable pool of world layers.
    /// </summary>
    private readonly ObjectPool<WorldLayer> layerPool = new();

    private readonly ILogger<WorldLayerGrouper> logger;

    private WorldLayer? activeLayer;

    /// <summary>
    ///     Blocks to render in the solid geometry for global lighting.
    /// </summary>
    public List<Vector3> SolidBlocks = new();

    /// <summary>
    ///     Blocks to render in the solid geometry for point lights, indexed by the per-frame light index.
    /// </summary>
    public List<List<uint>> SolidBlocksPerLight = new();

    public WorldLayerGrouper(ILogger<WorldLayerGrouper> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    ///     Active layers.
    /// </summary>
    public SortedDictionary<int, WorldLayer> Layers { get; }
        = new();

    /// <summary>
    ///     Clears the layers and returns them to the pool for reuse.
    /// </summary>
    public void ResetLayers()
    {
        activeLayer = null;
        foreach (var layer in Layers.Values)
        {
            layer.Reset();
            layerPool.ReturnObject(layer);
        }

        Layers.Clear();
        SolidBlocks.Clear();
        foreach (var blockList in SolidBlocksPerLight) blockList.Clear();
    }

    /// <summary>
    ///     Selects the Z depth used for any following calls to AddSprites.
    /// </summary>
    /// <param name="zFloor">Z floor.</param>
    public void SelectZDepth(int zFloor)
    {
        if (activeLayer != null && activeLayer.ZFloor == zFloor) return;

        activeLayer = SelectLayer(zFloor);
    }

    /// <summary>
    ///     Adds sprites to the appropriate layer while grouping them by entity type.
    /// </summary>
    /// <param name="perspectiveEntityType">Entity type.</param>
    /// <param name="position">Entity position.</param>
    /// <param name="velocity">Entity velocity.</param>
    /// <param name="sprite">Sprite.</param>
    /// <param name="lightFactor">Light factor.</param>
    /// <param name="opacity">Opacity alpha factor.</param>
    public void AddSprite(PerspectiveEntityType perspectiveEntityType, Vector3 position, Vector3 velocity,
        Sprite sprite, float lightFactor, float opacity)
    {
        if (activeLayer is null)
        {
            logger.LogError("No active layer.");
            return;
        }

        var collection = perspectiveEntityType switch
        {
            PerspectiveEntityType.NonBlock => activeLayer.FreeSprites,
            PerspectiveEntityType.BlockTopFace => activeLayer.TopFaceTileSprites,
            PerspectiveEntityType.BlockFrontFace => activeLayer.FrontFaceTileSprites,
            _ => activeLayer.FreeSprites
        };

        collection.Add(new PosVelId
        {
            Position = position,
            Velocity = velocity,
            Id = sprite.Id,
            LightFactor = lightFactor,
            Opacity = opacity
        });
    }

    /// <summary>
    ///     Adds a block for solid geometry rendering for global lighting.
    /// </summary>
    /// <param name="position">Block position.</param>
    public void AddSolidBlock(Vector3 position)
    {
        SolidBlocks.Add(position);
    }

    /// <summary>
    ///     Adds a block for solid geometry rendering for the given point light source.
    /// </summary>
    /// <param name="lightIndex">Per-frame light index.</param>
    /// <param name="solidBlockIndex">Index of the solid block being added.</param>
    public void AddSolidBlockForLight(int lightIndex, uint solidBlockIndex)
    {
        while (SolidBlocksPerLight.Count <= lightIndex) SolidBlocksPerLight.Add(new List<uint>());
        SolidBlocksPerLight[lightIndex].Add(solidBlockIndex);
    }

    /// <summary>
    ///     Selects the world layer based on the minimum z value.
    /// </summary>
    /// <param name="zFloor">Minimum z value of the layer.</param>
    /// <returns>World layer.</returns>
    private WorldLayer SelectLayer(int zFloor)
    {
        var found = Layers.TryGetValue(zFloor, out var layer);
        if (!found)
        {
            layer = layerPool.TakeObject();
            layer.ZFloor = zFloor;
            Layers[zFloor] = layer;
        }

        return layer!;
    }
}