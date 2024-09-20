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
using System.Numerics;
using Sovereign.ClientCore.Components;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Systems.Block.Caches;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Retrieves drawable entities for world rendering.
/// </summary>
public sealed class WorldEntityRetriever
{
    private readonly AnimatedSpriteManager animatedSpriteManager;
    private readonly AnimatedSpriteComponentCollection animatedSprites;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly BlockAnimatedSpriteCache blockSpriteCache;
    private readonly CameraManager camera;
    private readonly ClientConfigurationManager configManager;
    private readonly DrawableTagCollection drawableTags;
    private readonly WorldLayerGrouper grouper;

    /// <summary>
    ///     Half of the viewport width as a multiple of the tile width.
    /// </summary>
    private readonly float halfX;

    /// <summary>
    ///     Half of the viewport height as a multiple of the tile height.
    /// </summary>
    private readonly float halfY;

    private readonly KinematicComponentCollection kinematics;
    private readonly OrientationComponentCollection orientations;

    private readonly PerspectiveServices perspectiveServices;
    private readonly AnimationPhaseComponentCollection phases;

    public WorldEntityRetriever(CameraManager camera, DisplayViewport viewport,
        ClientConfigurationManager configManager, PerspectiveServices perspectiveServices,
        WorldLayerGrouper grouper, KinematicComponentCollection kinematics,
        BlockPositionComponentCollection blockPositions,
        AnimatedSpriteComponentCollection animatedSprites,
        DrawableTagCollection drawableTags, AnimatedSpriteManager animatedSpriteManager,
        OrientationComponentCollection orientations, AnimationPhaseComponentCollection phases,
        BlockAnimatedSpriteCache blockSpriteCache)
    {
        this.camera = camera;
        this.configManager = configManager;
        this.perspectiveServices = perspectiveServices;
        this.grouper = grouper;
        this.kinematics = kinematics;
        this.blockPositions = blockPositions;
        this.animatedSprites = animatedSprites;
        this.drawableTags = drawableTags;
        this.animatedSpriteManager = animatedSpriteManager;
        this.orientations = orientations;
        this.phases = phases;
        this.blockSpriteCache = blockSpriteCache;

        halfX = viewport.WidthInTiles * 0.5f;
        halfY = viewport.HeightInTiles * 0.5f;
    }

    /// <summary>
    ///     Retrieves the drawable entities for world rendering.
    /// </summary>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    /// <param name="systemTime">System time of current frame.</param>
    public void RetrieveEntities(float timeSinceTick, ulong systemTime)
    {
        grouper.ResetLayers();

        DetermineExtents(out var minExtent, out var maxExtent, timeSinceTick);

        var clientConfiguration = configManager.ClientConfiguration;
        var zMin = EntityList.ForComparison(
            (float)Math.Floor(minExtent.Z - halfY - clientConfiguration.RenderSearchSpacerY));
        var zMax = EntityList.ForComparison(
            (float)Math.Floor(minExtent.Z + halfY + clientConfiguration.RenderSearchSpacerY));

        for (var x = minExtent.X; x <= maxExtent.X; ++x)
        for (var y = minExtent.Y; y <= maxExtent.Y; ++y)
        {
            var intersectingPos = new GridPosition(x, y, minExtent.Z);
            if (!perspectiveServices.TryGetPerspectiveLine(intersectingPos, out var line))
                continue;

            ProcessPerspectiveLine(line, zMin, zMax, systemTime);
        }
    }

    /// <summary>
    ///     Processes a single perspective line.
    /// </summary>
    /// <param name="perspectiveLine">Perspective line.</param>
    /// <param name="zMin">Minimum of Z extent for rendering.</param>
    /// <param name="zMax">Maximum of Z extent for rendering.</param>
    /// <param name="systemTime">System time of current frame.</param>
    private void ProcessPerspectiveLine(PerspectiveLine perspectiveLine, EntityList zMin, EntityList zMax,
        ulong systemTime)
    {
        var foundOpaqueBlock = false;
        var searchSet = perspectiveLine.ZDepths.GetViewBetween(zMin, zMax).Reverse();
        foreach (var zSet in searchSet)
        {
            grouper.SelectZDepth(zSet.Z);

            var opaqueThisDepth = foundOpaqueBlock;
            foreach (var entity in zSet.Entities)
                // We make a few optimizations here based on the idea that opaque sprites will obscure anything
                // drawn below them. Blocks drawn on a perspective line perfectly overlap, so we only need to draw
                // blocks to the depth of the first encountered opaque sprite (if any).
                //
                // On the other hand, sprites may not be perfectly overlapped by an obscuring block sprite, and they
                // may not fully overlap any other sprites or block sprites below them. Accordingly, we draw all
                // animated sprites on the line regardless of if they may be obscured. To avoid multiple draws of the
                // same sprite, we only draw animated sprites for which the top-left corner of the sprite is on the
                // perspective line.
                //
                switch (entity.EntityType)
                {
                    case EntityType.NonBlock:
                        if (entity.OriginOnLine) ProcessSprite(entity.EntityId, systemTime);
                        break;

                    case EntityType.BlockTopFace:
                        if (!foundOpaqueBlock) ProcessBlockTopFace(entity.EntityId, systemTime, out opaqueThisDepth);
                        break;

                    case EntityType.BlockFrontFace:
                        if (!foundOpaqueBlock) ProcessBlockFrontFace(entity.EntityId, systemTime, out opaqueThisDepth);
                        break;
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
    private void ProcessSprite(ulong entityId, ulong systemTime)
    {
        if (!drawableTags.HasTagForEntity(entityId)) return;

        var entityKinematics = kinematics[entityId];
        var animatedSpriteId = animatedSprites[entityId];
        var orientation = orientations.HasComponentForEntity(entityId) ? orientations[entityId] : Orientation.South;
        var phase = phases.HasComponentForEntity(entityId) ? phases[entityId] : AnimationPhase.Default;

        var animatedSprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId];
        var sprite = animatedSprite.GetPhaseData(phase).GetSpriteForTime(systemTime, orientation);
        grouper.AddSprite(EntityType.NonBlock, entityKinematics.Position, entityKinematics.Velocity, sprite);
    }

    /// <summary>
    ///     Processes the front face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockFrontFace(ulong entityId, ulong systemTime, out bool isOpaque)
    {
        isOpaque = false;
        if (!drawableTags.HasTagForEntity(entityId)) return;

        var blockPosition = blockPositions[entityId];
        var facePosition = (Vector3)(blockPosition with { Y = blockPosition.Y - 1 });
        var animatedSpriteIds = blockSpriteCache.GetFrontFaceAnimatedSpriteIds(entityId);
        foreach (var animatedSpriteId in animatedSpriteIds)
        {
            var sprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].GetPhaseData(AnimationPhase.Default)
                .GetSpriteForTime(systemTime, Orientation.South);
            grouper.AddSprite(EntityType.BlockFrontFace, facePosition, Vector3.Zero, sprite);
            isOpaque = isOpaque || sprite.Opaque;
        }
    }

    /// <summary>
    ///     Processes the top face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockTopFace(ulong entityId, ulong systemTime, out bool isOpaque)
    {
        isOpaque = false;
        if (!drawableTags.HasTagForEntity(entityId)) return;

        var blockPosition = (Vector3)blockPositions[entityId];
        var animatedSpriteIds = blockSpriteCache.GetFrontFaceAnimatedSpriteIds(entityId);
        foreach (var animatedSpriteId in animatedSpriteIds)
        {
            var sprite = animatedSpriteManager.AnimatedSprites[animatedSpriteId].GetPhaseData(AnimationPhase.Default)
                .GetSpriteForTime(systemTime, Orientation.South);
            grouper.AddSprite(EntityType.BlockTopFace, blockPosition, Vector3.Zero, sprite);
            isOpaque = isOpaque || sprite.Opaque;
        }
    }

    /// <summary>
    ///     Determines the search extents as a 2D grid of block positions. The perspective lines
    ///     which intersect this grid will be searched for drawable entities.
    /// </summary>
    /// <param name="minExtent">Minimum extent.</param>
    /// <param name="maxExtent">Maximum extent.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    private void DetermineExtents(out GridPosition minExtent, out GridPosition maxExtent,
        float timeSinceTick)
    {
        /* Interpolate the camera position */
        var centerPos = camera.Position.InterpolateByTime(camera.Velocity, timeSinceTick);

        var clientConfiguration = configManager.ClientConfiguration;
        var minX = (int)Math.Floor(centerPos.X - halfX - clientConfiguration.RenderSearchSpacerX);
        var maxX = (int)Math.Floor(centerPos.X + halfX + clientConfiguration.RenderSearchSpacerX);

        var minY = (int)Math.Ceiling(centerPos.Y - halfY - clientConfiguration.RenderSearchSpacerY);
        var maxY = (int)Math.Ceiling(centerPos.Y + halfY + clientConfiguration.RenderSearchSpacerY);

        var z = (int)Math.Floor(centerPos.Z);

        minExtent = new GridPosition(minX, minY, z);
        maxExtent = new GridPosition(maxX, maxY, z);
    }
}