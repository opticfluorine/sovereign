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
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.ClientCore.Systems.Perspective;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Retrieves drawable entities for world rendering.
/// </summary>
public sealed class WorldEntityRetriever
{
    private readonly CameraManager camera;
    private readonly ClientConfigurationManager configManager;
    private readonly WorldLayerGrouper grouper;

    /// <summary>
    ///     Half of the viewport width as a multiple of the tile width.
    /// </summary>
    private readonly float halfX;

    /// <summary>
    ///     Half of the viewport height as a multiple of the tile height.
    /// </summary>
    private readonly float halfY;

    private readonly PerspectiveServices perspectiveServices;

    public WorldEntityRetriever(CameraManager camera, DisplayViewport viewport,
        ClientConfigurationManager configManager, PerspectiveServices perspectiveServices,
        WorldLayerGrouper grouper)
    {
        this.camera = camera;
        this.configManager = configManager;
        this.perspectiveServices = perspectiveServices;
        this.grouper = grouper;

        halfX = viewport.WidthInTiles * 0.5f;
        halfY = viewport.HeightInTiles * 0.5f;
    }

    /// <summary>
    ///     Retrieves the drawable entities for world rendering.
    /// </summary>
    /// <param name="entityBuffer">Drawable entity buffer.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    public void RetrieveEntities(List<PositionedEntity> entityBuffer, float timeSinceTick)
    {
        grouper.ResetLayers();

        DetermineExtents(out var minExtent, out var maxExtent, timeSinceTick);

        var clientConfiguration = configManager.ClientConfiguration;
        var zMin = EntityList.ForComparison(
            (float)Math.Floor(minExtent.Z - halfY - clientConfiguration.RenderSearchSpacerY));
        var zMax = EntityList.ForComparison(
            (float)Math.Floor(minExtent.Z + halfY + clientConfiguration.RenderSearchSpacerY));

        for (var x = minExtent.X; x <= maxExtent.X; ++x)
        {
            for (var y = minExtent.Y; y <= maxExtent.Y; ++y)
            {
                var intersectingPos = new GridPosition(x, y, minExtent.Z);
                if (!perspectiveServices.TryGetPerspectiveLine(intersectingPos, out var line))
                    continue;

                ProcessPerspectiveLine(entityBuffer, line, intersectingPos, zMin, zMax);
            }
        }
    }

    /// <summary>
    ///     Processes a single perspective line.
    /// </summary>
    /// <param name="entityBuffer"></param>
    /// <param name="perspectiveLine"></param>
    /// <param name="intersectingPos"></param>
    /// <param name="zMin"></param>
    /// <param name="zMax"></param>
    private void ProcessPerspectiveLine(List<PositionedEntity> entityBuffer, PerspectiveLine perspectiveLine,
        GridPosition intersectingPos, EntityList zMin, EntityList zMax)
    {
        var foundOpaqueBlock = false;
        var searchSet = perspectiveLine.ZDepths.GetViewBetween(zMin, zMax).Reverse();
        foreach (var zSet in searchSet)
        {
            var opaqueThisDepth = foundOpaqueBlock;
            foreach (var entity in zSet.Entities)
            {
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
                        ProcessSprite(entity.EntityId);
                        break;

                    case EntityType.BlockTopFace:
                        if (!foundOpaqueBlock) ProcessBlockTopFace(entity.EntityId, out opaqueThisDepth);
                        break;

                    case EntityType.BlockFrontFace:
                        if (!foundOpaqueBlock) ProcessBlockFrontFace(entity.EntityId, out opaqueThisDepth);
                        break;
                }
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
    private void ProcessSprite(ulong entityId)
    {
    }

    /// <summary>
    ///     Processes the front face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockFrontFace(ulong entityId, out bool isOpaque)
    {
        isOpaque = false;
    }

    /// <summary>
    ///     Processes the top face of a block on a perspective line.
    /// </summary>
    private void ProcessBlockTopFace(ulong entityId, out bool isOpaque)
    {
        isOpaque = false;
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