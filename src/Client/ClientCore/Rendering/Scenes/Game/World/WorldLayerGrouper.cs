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
using Castle.Core.Logging;
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

    private WorldLayer? activeLayer;

    /// <summary>
    ///     Blocks to render in the solid geometry.
    /// </summary>
    public List<Vector3> SolidBlocks = new();

    public ILogger Logger { private get; set; } = NullLogger.Instance;

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
    /// <param name="entityType">Entity type.</param>
    /// <param name="position">Entity position.</param>
    /// <param name="velocity">Entity velocity.</param>
    /// <param name="sprite">Sprite.</param>
    public void AddSprite(EntityType entityType, Vector3 position, Vector3 velocity, Sprite sprite)
    {
        if (activeLayer is null)
        {
            Logger.Error("No active layer.");
            return;
        }

        var collection = entityType switch
        {
            EntityType.NonBlock => activeLayer.AnimatedSprites,
            EntityType.BlockTopFace => activeLayer.TopFaceTileSprites,
            EntityType.BlockFrontFace => activeLayer.FrontFaceTileSprites,
            _ => activeLayer.AnimatedSprites
        };

        collection.Add(new PosVelId
        {
            Position = position,
            Velocity = velocity,
            Id = sprite.Id
        });
    }

    /// <summary>
    ///     Adds a block for solid geometry rendering.
    /// </summary>
    /// <param name="position">Block position.</param>
    public void AddSolidBlock(Vector3 position)
    {
        SolidBlocks.Add(position);
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