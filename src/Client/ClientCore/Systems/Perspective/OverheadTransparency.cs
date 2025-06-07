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
using Microsoft.Extensions.Logging;
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
    ClientStateServices clientStateServices,
    ILogger<OverheadTransparency> logger,
    OverheadBlockGraphManager graphManager)
{
    private const float InterpolationThreshold = 0.01f;

    private UnionFind2dGrid graph0 = new();
    private UnionFind2dGrid graph1 = new();
    private bool interpolating;
    private bool isActive;
    private float playerZ;

    /// <summary>
    ///     Sets up overhead transparency for the next frame.
    /// </summary>
    public void BeginFrame()
    {
        isActive = true;

        // Retrieve necessary player state. If any is missing, disable overhead transparency for this frame.
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId))
        {
            isActive = false;
            return;
        }

        if (!kinematics.TryGetValue(playerId, out var posVel))
        {
            logger.LogWarning("No position for player; overhead transparency disabled.");
            isActive = false;
            return;
        }

        graphManager.BeginFrame();

        playerZ = posVel.Position.Z;
        var z0 = (int)Math.Floor(playerZ);
        var z1 = (int)Math.Ceiling(playerZ);
        interpolating = z1 > z0 && playerZ - z0 > InterpolationThreshold;

        if (!graphManager.TryGetGraphForZ(z0, out var maybeGraph0))
        {
            logger.LogWarning("No graph for z {Z}; overhead transparency disabled.", z0);
            isActive = false;
            return;
        }

        graph0 = maybeGraph0;

        if (interpolating)
        {
            if (graphManager.TryGetGraphForZ(z1, out var maybeGraph1))
            {
                graph1 = maybeGraph1;
            }
            else
            {
                logger.LogWarning("No graph for higher z {Z}; interpolation disabled.", z1);
                interpolating = false;
            }
        }
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
        }

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
        // TODO
        return 1.0f;
    }
}