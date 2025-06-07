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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Sprites;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Retrieves drawable entities for world rendering.
/// </summary>
public sealed class WorldEntityRetriever
{
    private const float OpacityCullThreshold = 0.01f;

    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly AtlasMap atlasMap;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly IBlockAnimatedSpriteCache blockSpriteCache;
    private readonly CameraServices camera;
    private readonly CastBlockShadowsTagCollection castBlockShadows;
    private readonly CastShadowsComponentCollection castsShadows;
    private readonly WorldLayerGrouper grouper;

    private readonly KinematicsComponentCollection kinematics;

    public readonly List<PositionedLight> Lights = new();
    private readonly LightSourceTable lightSourceTable;
    private readonly ILogger<WorldEntityRetriever> logger;
    public readonly List<NameLabel> NameLabels = new();
    private readonly OrientationComponentCollection orientations;
    private readonly IPerspectiveController perspectiveController;

    private readonly IPerspectiveServices perspectiveServices;
    private readonly AnimationPhaseComponentCollection phases;
    private readonly PlayerCharacterTagCollection playerCharacters;
    private readonly WorldRangeSelector rangeSelector;

    private readonly RendererOptions rendererOptions;
    private readonly NonBlockShadowPlanner shadowPlanner;
    private readonly DisplayViewport viewport;

    /// <summary>
    ///     Number of solid blocks added so far in a frame.
    /// </summary>
    private uint solidBlockIndex;

    public WorldEntityRetriever(CameraServices camera, DisplayViewport viewport,
        IPerspectiveServices perspectiveServices, IPerspectiveController perspectiveController,
        WorldLayerGrouper grouper, KinematicsComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions,
        AnimatedSpriteComponentCollection animatedSprites,
        AnimatedSpriteManager animatedSpriteManager,
        OrientationComponentCollection orientations, AnimationPhaseComponentCollection phases,
        IBlockAnimatedSpriteCache blockSpriteCache, CastBlockShadowsTagCollection castBlockShadows,
        LightSourceTable lightSourceTable, PlayerCharacterTagCollection playerCharacters,
        WorldRangeSelector rangeSelector, AtlasMap atlasMap,
        ILogger<WorldEntityRetriever> logger, CastShadowsComponentCollection castsShadows,
        NonBlockShadowPlanner shadowPlanner, IOptions<RendererOptions> rendererOptions)
    {
        this.camera = camera;
        this.viewport = viewport;
        this.perspectiveServices = perspectiveServices;
        this.perspectiveController = perspectiveController;
        this.grouper = grouper;
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.animatedSprites = animatedSprites;
        this.animatedSpriteManager = animatedSpriteManager;
        this.orientations = orientations;
        this.phases = phases;
        this.blockSpriteCache = blockSpriteCache;
        this.castBlockShadows = castBlockShadows;
        this.lightSourceTable = lightSourceTable;
        this.playerCharacters = playerCharacters;
        this.rangeSelector = rangeSelector;
        this.atlasMap = atlasMap;
        this.logger = logger;
        this.castsShadows = castsShadows;
        this.shadowPlanner = shadowPlanner;
        this.rendererOptions = rendererOptions.Value;
    }

    /// <summary>
    ///     Retrieves the drawable entities for world rendering.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of current frame.</param>
    /// <param name="renderPlan">Render plan.</param>
    public void RetrieveEntities(float timeSinceTick, ulong systemTime, RenderPlan renderPlan)
    {
        grouper.ResetLayers();
        NameLabels.Clear();
        solidBlockIndex = 0;

        perspectiveController.BeginFrameSync(timeSinceTick);

        var halfY = viewport.HeightInTiles * 0.5f;

        var cameraPos = camera.Position.InterpolateByTime(camera.Velocity, timeSinceTick);

        rangeSelector.DetermineExtents(out var minExtent, out var maxExtent, cameraPos);

        var zMin = EntityList.ForComparison(
            (int)Math.Floor(minExtent.Z - halfY - rendererOptions.RenderSearchSpacerY));
        var zMax = EntityList.ForComparison(
            (int)Math.Floor(minExtent.Z + halfY + rendererOptions.RenderSearchSpacerY));

        // Identify light sources. This must be done at this point so that blocks can be flagged
        // for inclusion in the per-light shadow maps appropriately.
        Lights.Clear();
        var searchSpaceMin = new Vector3(minExtent.X, minExtent.Y, zMin.ZFloor);
        var searchSpaceMax = new Vector3(maxExtent.X, maxExtent.Y, zMax.ZFloor);
        lightSourceTable.GetLightsInRange(searchSpaceMin, searchSpaceMax, timeSinceTick, Lights);

        for (var x = minExtent.X; x <= maxExtent.X; ++x)
        for (var y = minExtent.Y; y <= maxExtent.Y; ++y)
        {
            var intersectingPos = new GridPosition(x, y, minExtent.Z);
            if (!perspectiveServices.TryGetPerspectiveLine(intersectingPos, out var line))
                continue;

            ProcessPerspectiveLine(line, zMin, zMax, systemTime, timeSinceTick, renderPlan);
        }
    }

    /// <summary>
    ///     Processes a single perspective line.
    /// </summary>
    /// <param name="perspectiveLine">Perspective line.</param>
    /// <param name="zMin">Minimum of Z extent for rendering.</param>
    /// <param name="zMax">Maximum of Z extent for rendering.</param>
    /// <param name="systemTime">System time of current frame.</param>
    /// <param name="timeSinceTick">Time since last tick, in seconds.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void ProcessPerspectiveLine(PerspectiveLine perspectiveLine, EntityList zMin, EntityList zMax,
        ulong systemTime, float timeSinceTick, RenderPlan renderPlan)
    {
        var foundOpaqueBlock = false;
        for (var i = 0; i < perspectiveLine.ZFloors.Count; ++i)
        {
            var zSet = perspectiveLine.ZFloors[i];
            if (zSet.ZFloor > zMax.ZFloor) continue;
            if (zSet.ZFloor < zMin.ZFloor) break;

            grouper.SelectZDepth(zSet.ZFloor);

            var opaqueThisDepth = foundOpaqueBlock;
            var frontFaceId = ulong.MaxValue;
            var topFaceId = ulong.MaxValue;

            // We make a few optimizations here based on the idea that opaque sprites will obscure anything
            // drawn below them. Blocks drawn on a perspective line perfectly overlap, so we only need to draw
            // blocks to the depth of the first encountered opaque sprite (if any).
            //
            // On the other hand, sprites may not be perfectly overlapped by an obscuring block sprite, and they
            // may not fully overlap any other sprites or block sprites below them. Accordingly, we draw all
            // animated sprites on the line regardless of if they may be obscured. To avoid multiple draws of the
            // same sprite, we only draw animated sprites for which the top-left corner of the sprite is on the
            // perspective line.

            // First pass, pull out the block faces so they can be ordered and checked appropriately.
            for (var j = zSet.Entities.Count - 1; j >= 0; j--)
            {
                var entity = zSet.Entities[j];
                switch (entity.EntityType)
                {
                    case EntityType.BlockFrontFace:
                        frontFaceId = entity.EntityId;
                        break;

                    case EntityType.BlockTopFace:
                        topFaceId = entity.EntityId;
                        break;
                }

                if (frontFaceId < ulong.MaxValue && topFaceId < ulong.MaxValue) break;
            }

            // Between passes, handle the faces.
            if (!foundOpaqueBlock && frontFaceId < ulong.MaxValue)
                ProcessBlockFrontFace(frontFaceId, systemTime, out opaqueThisDepth);

            if (!foundOpaqueBlock && !opaqueThisDepth && topFaceId < ulong.MaxValue)
                ProcessBlockTopFace(topFaceId, systemTime, out opaqueThisDepth);

            // If a top face was found, mark the block for solid geometry rendering.
            if (topFaceId < ulong.MaxValue) ProcessSolidBlock(topFaceId);

            // Second pass, sending entities to the layer grouper.
            for (var j = zSet.Entities.Count - 1; j >= 0; j--)
            {
                // Skip block faces since they were already handled above.
                var entity = zSet.Entities[j];
                if (entity.EntityType != EntityType.NonBlock) continue;
                if (entity.OriginOnLine) ProcessSprite(entity.EntityId, systemTime, timeSinceTick, renderPlan);
            }

            // Opaque blocking is updated at the end of each z-depth since a front face and top face
            // can both appear at the same z-depth, but the order in which they appear in zSet is
            // arbitrary.
            foundOpaqueBlock = opaqueThisDepth;
        }
    }

    /// <summary>
    ///     Processes an animated sprite on a perspective line.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="systemTime">System time of current frame in ticks.</param>
    /// <param name="timeSinceTick">Time since the last tick in seconds.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void ProcessSprite(ulong entityId, ulong systemTime, float timeSinceTick, RenderPlan renderPlan)
    {
        var opacity = perspectiveServices.GetOpacityForEntity(entityId);
        if (opacity < OpacityCullThreshold) return;

        var entityKinematics = kinematics[entityId];
        var animatedSpriteId = animatedSprites[entityId];
        var orientation = orientations.HasComponentForEntity(entityId) ? orientations[entityId] : Orientation.South;
        var phase = phases.HasComponentForEntity(entityId) ? phases[entityId] : AnimationPhase.Default;

        var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
        var sprite = animatedSprite.GetPhaseData(phase).GetSpriteForTime(systemTime, orientation);
        grouper.AddSprite(EntityType.NonBlock, entityKinematics.Position, entityKinematics.Velocity, sprite, 1.0f,
            opacity);

        if (playerCharacters.HasTagForEntity(entityId))
            AddNameLabel(entityId, entityKinematics, sprite, timeSinceTick);
        if (castsShadows.TryGetValue(entityId, out var shadow))
            shadowPlanner.AddNonBlockShadow(renderPlan, shadow, entityKinematics, sprite.Id);
    }

    /// <summary>
    ///     Adds a name label for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="entityKinematics">Kinematics for entity.</param>
    /// <param name="sprite">Current sprite for entity.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    private void AddNameLabel(ulong entityId, Kinematics entityKinematics, Sprite sprite, float timeSinceTick)
    {
        // The name label should appear above the sprite, so translate it upward by the sprite height.
        var spriteInfo = atlasMap.MapElements[sprite.Id];
        var zOffset = new Vector3(0.0f, 0.0f, spriteInfo.HeightInTiles);

        NameLabels.Add(new NameLabel
        {
            EntityId = entityId,
            InterpolatedPosition = entityKinematics.Position + zOffset + timeSinceTick * entityKinematics.Velocity,
            SpriteId = sprite.Id
        });
    }

    /// <summary>
    ///     Processes the front face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockFrontFace(ulong entityId, ulong systemTime, out bool isOpaque)
    {
        isOpaque = false;

        if (!blockPositions.TryGetValue(entityId, out var blockPosition))
        {
            logger.LogWarning("Block {EntityId:X} not found.", entityId);
            return;
        }

        var opacity = perspectiveServices.GetOpacityForEntity(entityId);
        if (opacity < OpacityCullThreshold) return;

        var facePosition = (Vector3)blockPosition;
        var animatedSpriteIds = blockSpriteCache.GetFrontFaceAnimatedSpriteIds(entityId);
        var firstLayer = true;
        foreach (var animatedSpriteId in animatedSpriteIds)
        {
            var sprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].GetPhaseData(AnimationPhase.Default)
                .GetSpriteForTime(systemTime, Orientation.South);
            grouper.AddSprite(EntityType.BlockFrontFace, facePosition, Vector3.Zero, sprite,
                firstLayer ? 1.0f : 0.0f, opacity);
            isOpaque = isOpaque || sprite.Opaque;
            firstLayer = false;
        }
    }

    /// <summary>
    ///     Processes the top face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockTopFace(ulong entityId, ulong systemTime, out bool isOpaque)
    {
        isOpaque = false;

        if (!blockPositions.TryGetValue(entityId, out var blockPosition))
        {
            logger.LogWarning("Block {EntityId:X} not found.", entityId);
            return;
        }

        var opacity = perspectiveServices.GetOpacityForEntity(entityId);
        if (opacity < OpacityCullThreshold) return;

        var facePosition = (Vector3)(blockPosition with { Z = blockPosition.Z + 1 });
        var animatedSpriteIds = blockSpriteCache.GetTopFaceAnimatedSpriteIds(entityId);
        var firstLayer = true;
        foreach (var animatedSpriteId in animatedSpriteIds)
        {
            var sprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].GetPhaseData(AnimationPhase.Default)
                .GetSpriteForTime(systemTime, Orientation.South);
            grouper.AddSprite(EntityType.BlockTopFace, facePosition, Vector3.Zero, sprite,
                firstLayer ? 1.0f : 0.0f, opacity);
            isOpaque = isOpaque || sprite.Opaque;
            firstLayer = false;
        }
    }

    /// <summary>
    ///     Processes a block for solid geometry rendering.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void ProcessSolidBlock(ulong entityId)
    {
        if (!castBlockShadows.HasTagForEntity(entityId)) return;

        if (!blockPositions.TryGetValue(entityId, out var blockGridPos))
        {
            logger.LogWarning("Block {EntityId:X} not found.", entityId);
            return;
        }

        var blockPosition = (Vector3)blockGridPos;

        // Global lighting.
        grouper.AddSolidBlock(blockPosition);

        // Check individual lights to see if this block is in range.
        foreach (var light in Lights)
        {
            // Check whether in range.
            // Use a larger radius to ensure blocks which partially overlap the lighted volume.
            var displacement = blockPosition - light.Position;
            var wideRadius = 2.0f * light.Details.Radius + 1.0f;
            if (displacement.LengthSquared() > wideRadius * wideRadius) continue;

            // Block is in range, tag it for local lighting calculations.
            grouper.AddSolidBlockForLight(light.Index, solidBlockIndex);
        }

        solidBlockIndex++;
    }
}