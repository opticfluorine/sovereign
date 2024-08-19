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
using Sovereign.ClientCore.Rendering.Materials;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Collections;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Groups drawables in range by their layer.
/// </summary>
public sealed class WorldLayerGrouper
{
    private readonly AboveBlockComponentCollection aboveBlocks;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly KinematicComponentCollection kinematics;

    /// <summary>
    ///     Reusable pool of world layers.
    /// </summary>
    private readonly ObjectPool<WorldLayer> layerPool = new();

    private readonly MaterialManager materialManager;
    private readonly MaterialModifierComponentCollection materialModifiers;

    private readonly MaterialComponentCollection materials;
    private readonly OrientationComponentCollection orientations;

    public WorldLayerGrouper(MaterialComponentCollection materials,
        MaterialModifierComponentCollection materialModifiers,
        AnimatedSpriteComponentCollection animatedSprites,
        AboveBlockComponentCollection aboveBlocks,
        MaterialManager materialManager,
        OrientationComponentCollection orientations,
        KinematicComponentCollection kinematics)
    {
        this.materials = materials;
        this.materialModifiers = materialModifiers;
        this.animatedSprites = animatedSprites;
        this.aboveBlocks = aboveBlocks;
        this.materialManager = materialManager;
        this.orientations = orientations;
        this.kinematics = kinematics;
    }

    public ILogger Logger { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Active layers.
    /// </summary>
    public SortedDictionary<int, WorldLayer> Layers { get; }
        = new();

    /// <summary>
    ///     Groups the drawables into their respective layers.
    /// </summary>
    /// <param name="drawables">Unordered list of drawable entities in range.</param>
    public void GroupDrawables(List<PositionedEntity> drawables)
    {
        ResetLayers();
        foreach (var drawable in drawables) ProcessDrawable(drawable);
    }

    /// <summary>
    ///     Processes a single drawable.
    /// </summary>
    /// <param name="drawable">Drawable to process.</param>
    private void ProcessDrawable(PositionedEntity drawable)
    {
        /* Get the entity velocity, defaulting to zero if not set. */
        var velocity = kinematics.HasComponentForEntity(drawable.EntityId)
            ? kinematics[drawable.EntityId].Velocity
            : Vector3.Zero;

        /* Route the drawable to the correct rendering list. */
        if (materials.HasComponentForEntity(drawable.EntityId))
            AddMaterial(drawable, materials[drawable.EntityId], velocity);
        else
            AddAnimatedSprite(drawable, velocity);
    }

    /// <summary>
    ///     Adds a non-material animated sprite to the rendering sequence.
    /// </summary>
    /// <param name="drawable">Drawable to process.</param>
    /// <param name="velocity">Velocity of the entity.</param>
    private void AddAnimatedSprite(PositionedEntity drawable, Vector3 velocity)
    {
        var zFloor = (int)drawable.Position.Z;
        var layer = SelectLayer(zFloor);
        var sprite = animatedSprites.GetComponentForEntity(drawable.EntityId);
        if (sprite.HasValue)
            layer.AnimatedSprites.Add(new PosVelId
            {
                Position = drawable.Position,
                Velocity = velocity,
                Id = sprite.Value,
                EntityId = drawable.EntityId,
                Orientation = orientations.GetComponentForEntity(drawable.EntityId)
                    .OrElseDefault(Orientation.South)
            });
    }

    /// <summary>
    ///     Adds a material block to the rendering sequence.
    /// </summary>
    /// <param name="drawable">Drawable to process.</param>
    /// <param name="materialId">Material ID.</param>
    /// <param name="velocity">Velocity of the entity.</param>
    private void AddMaterial(PositionedEntity drawable, int materialId, Vector3 velocity)
    {
        /* The current z contains the top face, the lower z contains the front face */
        var zFloorTop = (int)drawable.Position.Z;
        var zFloorFront = zFloorTop - 1;
        var layerTop = SelectLayer(zFloorTop);
        var layerFront = SelectLayer(zFloorFront);

        MaterialSubtype materialSubtype;
        try
        {
            materialSubtype = GetMaterialSubtype(drawable.EntityId, materialId);
        }
        catch
        {
            Logger.ErrorFormat("Invalid material/modifier combination for entity ID {0}",
                drawable.EntityId);
            return;
        }

        AddMaterialTopFace(drawable, materialSubtype, velocity, layerTop);
        AddMaterialFrontFace(drawable, materialSubtype, velocity, layerFront);
    }

    /// <summary>
    ///     Adds the front face of a material block to its rendering layer.
    /// </summary>
    /// <param name="drawable">Material block information.</param>
    /// <param name="materialSubtype">Material subtype.</param>
    /// <param name="velocity">Velocity of the entity.</param>
    /// <param name="layerFront">Rendering layer.</param>
    private void AddMaterialFrontFace(PositionedEntity drawable, MaterialSubtype materialSubtype,
        Vector3 velocity, WorldLayer layerFront)
    {
        layerFront.FrontFaceTileSprites.Add(new PosVelId
        {
            Position = drawable.Position with { Y = drawable.Position.Y - 1.0f },
            Velocity = velocity,
            Id = materialSubtype.SideFaceTileSpriteId,
            EntityId = drawable.EntityId
        });
    }

    /// <summary>
    ///     Adds the top face of a material block to its rendering layer.
    /// </summary>
    /// <param name="drawable">Material block information.</param>
    /// <param name="materialSubtype">Material subtype.</param>
    /// <param name="velocity">Velocity of the entity.</param>
    /// <param name="layerTop">Rendering layer.</param>
    private void AddMaterialTopFace(PositionedEntity drawable, MaterialSubtype materialSubtype,
        Vector3 velocity, WorldLayer layerTop)
    {
        var topFaceId = IsTopFaceObscured(drawable.EntityId)
            ? materialSubtype.ObscuredTopFaceTileSpriteId
            : materialSubtype.TopFaceTileSpriteId;
        layerTop.TopFaceTileSprites.Add(new PosVelId
        {
            Position = drawable.Position,
            Velocity = velocity,
            Id = topFaceId,
            EntityId = drawable.EntityId
        });
    }

    /// <summary>
    ///     Determines whether the top face of a block is obscured by another block.
    /// </summary>
    /// <param name="entityId">Entity ID of the block.</param>
    /// <returns>true if the top face is obscured, false otherwise.</returns>
    private bool IsTopFaceObscured(ulong entityId)
    {
        return aboveBlocks.HasComponentForEntity(entityId);
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

    /// <summary>
    ///     Looks up the material subtype for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="materialId">Material ID.</param>
    /// <returns>Material subtype.</returns>
    private MaterialSubtype GetMaterialSubtype(ulong entityId, int materialId)
    {
        var modifier = materialModifiers.HasComponentForEntity(entityId) ? materialModifiers[entityId] : 0;
        return materialManager.Materials[materialId].MaterialSubtypes[modifier];
    }

    /// <summary>
    ///     Clears the layers and returns them to the pool for reuse.
    /// </summary>
    private void ResetLayers()
    {
        foreach (var layer in Layers.Values)
        {
            layer.Reset();
            layerPool.ReturnObject(layer);
        }

        Layers.Clear();
    }
}