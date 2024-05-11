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
using System.Linq;
using System.Numerics;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Components.Indexers;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.World;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Retrieves drawable entities for world rendering.
/// </summary>
public sealed class WorldEntityRetriever
{
    private readonly BlockWorldSegmentIndexer blockIndexer;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly CameraManager camera;
    private readonly ClientConfigurationManager configManager;
    private readonly DrawablePositionComponentIndexer drawableIndexer;

    /// <summary>
    ///     Half of the viewport width as a multiple of the tile width.
    /// </summary>
    private readonly float halfX;

    /// <summary>
    ///     Half of the viewport height as a multiple of the tile height.
    /// </summary>
    private readonly float halfY;

    private readonly List<GridPosition> renderedWorldSegments = new();
    private readonly WorldSegmentResolver resolver;

    private readonly DisplayViewport viewport;

    public WorldEntityRetriever(DrawablePositionComponentIndexer drawableIndexer,
        CameraManager camera, DisplayViewport viewport, ClientConfigurationManager configManager,
        WorldSegmentResolver resolver, BlockWorldSegmentIndexer blockIndexer,
        BlockPositionComponentCollection blockPositions)
    {
        this.drawableIndexer = drawableIndexer;
        this.camera = camera;
        this.viewport = viewport;
        this.configManager = configManager;
        this.resolver = resolver;
        this.blockIndexer = blockIndexer;
        this.blockPositions = blockPositions;

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
        // Non-block entities.
        DetermineExtents(out var minExtent, out var maxExtent, timeSinceTick);
        using (var indexLock = drawableIndexer.AcquireLock())
        {
            drawableIndexer.GetEntitiesInRange(indexLock, minExtent, maxExtent, entityBuffer);
        }

        // Block entities.
        SelectWorldSegments(minExtent, maxExtent);
        entityBuffer.AddRange(renderedWorldSegments
            .SelectMany(segmentIndex => blockIndexer.GetEntitiesInWorldSegment(segmentIndex))
            .Select(entityId => new PositionedEntity
            {
                EntityId = entityId,
                Position = (Vector3)blockPositions[entityId]
            }));
    }

    /// <summary>
    ///     Determines the search extents.
    /// </summary>
    /// <param name="minExtent">Minimum extent.</param>
    /// <param name="maxExtent">Maximum extent.</param>
    /// <param name="timeSinceTick">Time since the last tick, in seconds.</param>
    private void DetermineExtents(out Vector3 minExtent, out Vector3 maxExtent,
        float timeSinceTick)
    {
        /* Interpolate the camera position */
        var centerPos = camera.Position.InterpolateByTime(camera.Velocity, timeSinceTick);

        var clientConfiguration = configManager.ClientConfiguration;
        var minX = centerPos.X - halfX - clientConfiguration.RenderSearchSpacerX;
        var maxX = centerPos.X + halfX + clientConfiguration.RenderSearchSpacerX;

        var minY = centerPos.Y - halfY - clientConfiguration.RenderSearchSpacerY;
        var maxY = centerPos.Y + halfY + clientConfiguration.RenderSearchSpacerY;

        /* z uses same spans as y */
        var minZ = centerPos.Z - halfY - clientConfiguration.RenderSearchSpacerY;
        var maxZ = centerPos.Z + halfY + clientConfiguration.RenderSearchSpacerY;

        minExtent = new Vector3(minX, minY, minZ);
        maxExtent = new Vector3(maxX, maxY, maxZ);
    }

    /// <summary>
    ///     Identifies the world segments that need to be considered for rendering.
    /// </summary>
    /// <param name="minExtent">Minimum extent of rendering.</param>
    /// <param name="maxExtent">Maximum extent of rendering.</param>
    private void SelectWorldSegments(Vector3 minExtent, Vector3 maxExtent)
    {
        var startSegment = resolver.GetWorldSegmentForPosition(minExtent);
        var endSegment = resolver.GetWorldSegmentForPosition(maxExtent);

        renderedWorldSegments.Clear();
        for (var x = startSegment.X; x <= endSegment.X; ++x)
        {
            for (var y = startSegment.Y; y <= endSegment.Y; ++y)
            {
                for (var z = startSegment.Z; z <= endSegment.Z; ++z)
                {
                    renderedWorldSegments.Add(new GridPosition(x, y, z));
                }
            }
        }
    }
}