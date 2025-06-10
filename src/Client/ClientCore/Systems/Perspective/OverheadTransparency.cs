// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Sprites.AnimatedSprites;
using Sovereign.ClientCore.Rendering.Sprites.Atlas;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineUtil.Algorithms;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Computes opacity factors for overhead transparency effects.
/// </summary>
internal class OverheadTransparency(
    KinematicsComponentCollection kinematics,
    BlockPositionComponentCollection blockPositions,
    AnimatedSpriteComponentCollection animatedSprites,
    ClientStateServices clientStateServices,
    ILogger<OverheadTransparency> logger,
    OverheadBlockGraphManager graphManager,
    AnimatedSpriteManager animatedSpriteManager,
    PerspectiveLineManager perspectiveLineManager,
    AtlasMap atlasMap)
{
    private const float InterpolationThreshold = 0.01f;
    private const int ZLimitSearchRadius = 3;

    /// <summary>
    ///     Map from (z, componentId) to the component's overlap with the player sprite.
    /// </summary>
    private readonly Dictionary<(int, int), float> componentOverlapCache = new();

    private readonly List<(int, int, float)> playerOverlaps = new();
    private bool debugFrame;

    private UnionFind2dGrid graph0 = new();
    private UnionFind2dGrid graph1 = new();
    private bool interpolating;
    private bool isActive;
    private int minimumZ;
    private ulong playerId;
    private float playerZ;

    /// <summary>
    ///     Sets up overhead transparency for the next frame.
    /// </summary>
    /// <param name="timeSinceTick">Time in seconds since the last tick.</param>
    public void BeginFrame(float timeSinceTick)
    {
        isActive = true;

        // Retrieve necessary player state. If any is missing, disable overhead transparency for this frame.
        debugFrame = clientStateServices.GetStateFlagValue(ClientStateFlag.DebugFrame);
        if (!clientStateServices.TryGetSelectedPlayer(out playerId))
        {
            if (debugFrame) logger.LogDebug("No selected player; overhead transparency disabled.");
            isActive = false;
            return;
        }

        // Clear caches.
        componentOverlapCache.Clear();

        if (!kinematics.TryGetValue(playerId, out var posVel))
        {
            logger.LogWarning("No position for player; overhead transparency disabled.");
            isActive = false;
            return;
        }

        var playerPosition = posVel.Position + timeSinceTick * posVel.Velocity;
        playerZ = playerPosition.Z;
        var z0 = (int)Math.Floor(playerZ);
        var z1 = (int)Math.Ceiling(playerZ);
        interpolating = z1 > z0 && playerZ - z0 > InterpolationThreshold;

        if (!graphManager.TryGetGraphForZ(z0, out var maybeGraph0))
        {
            logger.LogTrace("Disable overhead transparency; no graph for z {Z}.", z0);
            isActive = false;
            return;
        }

        graph0 = maybeGraph0;

        if (interpolating)
        {
            if (graphManager.TryGetGraphForZ(z1, out var maybeGraph1))
                graph1 = maybeGraph1;
            else
                interpolating = false;
        }

        if (debugFrame)
            logger.LogDebug("z0: {Z0}, z1: {Z1}, interpolating: {Interpolating}",
                z0, z1, interpolating);

        DeterminePlayerOverlaps(playerId, playerPosition);
        minimumZ = SelectMinimumZ(playerPosition);
    }

    /// <summary>
    ///     Gets the opacity factor for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Opacity factor.</returns>
    public float GetOpacityForEntity(ulong entityId)
    {
        if (!isActive) return 1.0f;

        // Get entity position clamped to the graph.
        if (!blockPositions.TryGetValue(entityId, out var entityPosition))
        {
            // Non-block entity.
            if (!kinematics.TryGetValue(entityId, out var posVel))
            {
                logger.LogWarning("No position for entity {EntityId}; returning full opacity.", entityId);
                return 1.0f;
            }

            entityPosition = new GridPosition(
                (int)Math.Floor(posVel.Position.X),
                (int)Math.Floor(posVel.Position.Y),
                (int)Math.Floor(posVel.Position.Z));

            // Non-block entities at same z layer as player are always fully opaque.
            if ((int)Math.Floor(playerZ) == entityPosition.Z) return 1.0f;
        }

        if (entityPosition.Z < minimumZ || entityId == playerId) return 1.0f;

        // Lower graph opacity.
        var opacity0 = GetOpacityFromGraph(entityPosition, graph0);
        if (!interpolating) return opacity0;

        // Upper graph opacity and linear interpolation.
        var opacity1 = GetOpacityFromGraph(entityPosition, graph1);
        return opacity0 + (opacity1 - opacity0) * (playerZ - (float)Math.Floor(playerZ));
    }

    /// <summary>
    ///     Gets the opacity factor for a block position from the given graph.
    /// </summary>
    /// <param name="entityPosition">Block position.</param>
    /// <param name="graph">Graph.</param>
    /// <returns>Opacity factor.</returns>
    private float GetOpacityFromGraph(GridPosition entityPosition, UnionFind2dGrid graph)
    {
        var latticePoint = (entityPosition.X, entityPosition.Y + entityPosition.Z);
        if (!graph.TryGetComponent(latticePoint, out var componentId)) return 1.0f;

        if (!componentOverlapCache.TryGetValue((entityPosition.Z, componentId), out var overlapFactor))
        {
            overlapFactor = ConputeComponentPlayerOverlap(graph, componentId);
            componentOverlapCache[(entityPosition.Z, componentId)] = overlapFactor;
        }

        return 1.0f - overlapFactor;
    }

    /// <summary>
    ///     Determines the overlap between the player and the perspective lattice.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="playerPosition">Interpolated player position.</param>
    private void DeterminePlayerOverlaps(ulong playerEntityId, Vector3 playerPosition)
    {
        playerOverlaps.Clear();

        // Get player draw info.
        var pos0 = new Vector2(playerPosition.X, playerPosition.Y + playerPosition.Z);
        if (!animatedSprites.TryGetValue(playerEntityId, out var animSpriteId))
        {
            logger.LogWarning("Player has no animated sprite; using simple overhead overlap.");
            playerOverlaps.Add(((int)Math.Floor(pos0.X), (int)Math.Floor(pos0.Y), 1.0f));
            return;
        }

        var animSprite = animatedSpriteManager.AnimatedSprites[animSpriteId];
        var spriteId = animSprite.GetDefaultSprite().Id;
        var spriteInfo = atlasMap.MapElements[spriteId];
        var pos1 = pos0 + new Vector2(spriteInfo.WidthInTiles, spriteInfo.HeightInTiles);

        var x0 = (int)Math.Floor(pos0.X);
        var y0 = (int)Math.Floor(pos0.Y);
        var x1 = (int)Math.Ceiling(pos1.X);
        var y1 = (int)Math.Ceiling(pos1.Y);

        var invNorm = 1.0f / (spriteInfo.WidthInTiles * spriteInfo.HeightInTiles);
        for (var x = x0; x < x1; ++x)
        for (var y = y0; y < y1; ++y)
        {
            var dx = 1.0f - Math.Max(0.0f, pos0.X - x) - Math.Max(0.0f, x + 1 - pos1.X);
            var dy = 1.0f - Math.Max(0.0f, pos0.Y - y) - Math.Max(0.0f, y + 1 - pos1.Y);
            var overlap = dx * dy * invNorm;
            playerOverlaps.Add((x, y, overlap));

            if (debugFrame) logger.LogDebug("Player overlap at ({X}, {Y}): {Overlap}", x, y, overlap);
        }
    }

    /// <summary>
    ///     Computes the overlap between the player sprite and a component of the given perspective lattice subgraph.
    /// </summary>
    /// <param name="graph">Graph.</param>
    /// <param name="componentId">Connected component ID.</param>
    /// <returns>Overlap factor.</returns>
    private float ConputeComponentPlayerOverlap(UnionFind2dGrid graph, int componentId)
    {
        var overlapSum = 0.0f;

        foreach (var overlapVertex in playerOverlaps)
            if (graph.TryGetComponent((overlapVertex.Item1, overlapVertex.Item2), out var compId) &&
                compId == componentId)
                overlapSum += overlapVertex.Item3;

        if (debugFrame) logger.LogDebug("Component {ComponentId} overlap: {Overlap}", componentId, overlapSum);
        return overlapSum;
    }

    /// <summary>
    ///     Selects the minimum Z at which overhead transparency should be applied.
    /// </summary>
    /// <param name="playerPosition">Player position.</param>
    /// <returns>Minimum Z at which transparency should be applied.</returns>
    private int SelectMinimumZ(Vector3 playerPosition)
    {
        var maxAbove = int.MinValue;
        var baseX = (int)Math.Floor(playerPosition.X);
        var baseY = (int)Math.Floor(playerPosition.Y + playerPosition.Z);
        var baseZ = (int)Math.Floor(playerZ);
        var directAbove = int.MaxValue;

        // Search an area around the player position to find the highest Z above the player.
        // Use this as a threshold. This eliminates some choppiness when passing through
        // doors and other areas.
        for (var dx = -ZLimitSearchRadius; dx <= ZLimitSearchRadius; ++dx)
        for (var dy = -ZLimitSearchRadius; dy <= ZLimitSearchRadius; ++dy)
        {
            var searchPoint = (baseX + dx, baseY + dy);
            if (!perspectiveLineManager.TryGetPerspectiveLine(searchPoint, out var line)) continue;
            if (!line.TryGetFirstZFloorWithBlockAbove(baseZ, out var lineZ, out var hasTopFace)) continue;

            // If there is a top face, also grab the front face in the z floor below.
            maxAbove = Math.Max(maxAbove, hasTopFace ? lineZ - 1 : lineZ);

            // Ensure that the threshold doesn't rise above the lowest layer of the ceiling.
            foreach (var (ox, oy, overlap) in playerOverlaps)
                if (ox == searchPoint.Item1 && oy == searchPoint.Item2 && overlap > 0.0f)
                    directAbove = Math.Min(lineZ, directAbove);
        }

        return maxAbove == int.MinValue ? baseZ + 1 : Math.Min(maxAbove, directAbove);
    }
}