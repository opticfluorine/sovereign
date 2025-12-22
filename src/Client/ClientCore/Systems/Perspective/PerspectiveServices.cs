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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Public read API for perspective information.
/// </summary>
public interface IPerspectiveServices
{
    /// <summary>
    ///     Gets the highest entity appearing to overlap a position within a given z window.
    /// </summary>
    /// <param name="position">Position in world coordinates to overlap.</param>
    /// <param name="minimumZ">Minimum Z for window.</param>
    /// <param name="maximumZ">Maximum Z for window.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if an entity overlapped, false otherwise.</returns>
    bool TryGetHighestCoveringEntity(Vector3 position, float minimumZ, float maximumZ, out ulong entityId);

    /// <summary>
    ///     Gets the highest entity appearing to overlap a position within a z window with the same
    ///     height as the current viewport.
    /// </summary>
    /// <param name="position">Position in world coordinates to overlap.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if an entity overlapped, false otherwise.</returns>
    bool TryGetHighestCoveringEntity(Vector3 position, out ulong entityId);

    /// <summary>
    ///     Gets the highest visible (i.e. no overhead transparency) block covering a position in world coordinates.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="entityType">Indicates which face of the block is overlapped.</param>
    /// <param name="positionOnBlock">Exact hovered position on the block face.</param>
    /// <returns>true if a visible block overlapped, false otherwise.</returns>
    bool TryGetHighestVisibleCoveringBlock(Vector3 position, out ulong entityId, out PerspectiveEntityType entityType,
        out Vector3 positionOnBlock);

    /// <summary>
    ///     Gets the perspective line (if any) on which the given block position sits.
    /// </summary>
    /// <param name="blockPosition">Block position on the perspective line.</param>
    /// <param name="perspectiveLine">Perspective line if one is found.</param>
    /// <returns>true if a perspective line was found, false otherwise.</returns>
    bool TryGetPerspectiveLine(GridPosition blockPosition,
        [NotNullWhen(true)] out PerspectiveLine? perspectiveLine);

    /// <summary>
    ///     Gets the opacity alpha factor for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Opacity alpha factor.</returns>
    float GetOpacityForEntity(ulong entityId);

    /// <summary>
    ///     Fills the given buffer with IDs of items found under the player.
    /// </summary>
    /// <param name="idBuffer">Buffer to fill.</param>
    /// <returns>Number of items added to the buffer.</returns>
    /// <remarks>
    ///     "Under the player" is defined in this context to mean that the item and the player have equal
    ///     z position and overlap the same perspective line. This does not guarantee that there is an overlap
    ///     between the player and item!
    /// </remarks>
    int GetItemsUnderPlayer(Span<ulong> idBuffer);
}

/// <summary>
///     Implementation of IPerspectiveServices.
/// </summary>
internal class PerspectiveServices : IPerspectiveServices
{
    private const float ZTol = 1E-5f;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly ClientStateServices clientStateServices;
    private readonly EntityTypeComponentCollection entityTypes;
    private readonly KinematicsComponentCollection kinematics;
    private readonly PerspectiveLineManager lineManager;
    private readonly ILogger<PerspectiveServices> logger;
    private readonly OverheadTransparency overheadTransparency;
    private readonly DisplayViewport viewport;

    public PerspectiveServices(PerspectiveLineManager lineManager, DisplayViewport viewport,
        OverheadTransparency overheadTransparency, BlockPositionComponentCollection blockPositions,
        ClientStateServices clientStateServices, KinematicsComponentCollection kinematics,
        EntityTypeComponentCollection entityTypes,
        ILogger<PerspectiveServices> logger)
    {
        this.lineManager = lineManager;
        this.viewport = viewport;
        this.overheadTransparency = overheadTransparency;
        this.blockPositions = blockPositions;
        this.clientStateServices = clientStateServices;
        this.kinematics = kinematics;
        this.entityTypes = entityTypes;
        this.logger = logger;
    }

    public bool TryGetHighestCoveringEntity(Vector3 position, float minimumZ, float maximumZ, out ulong entityId)
    {
        return lineManager.TryGetHighestEntityAtPoint(position, minimumZ, maximumZ, out entityId);
    }

    public bool TryGetHighestCoveringEntity(Vector3 position, out ulong entityId)
    {
        var minZ = position.Z - viewport.HeightInTiles * 0.5f;
        var maxZ = position.Z + viewport.HeightInTiles * 0.5f;
        return TryGetHighestCoveringEntity(position, minZ, maxZ, out entityId);
    }

    public bool TryGetHighestVisibleCoveringBlock(Vector3 position, out ulong entityId,
        out PerspectiveEntityType entityType, out Vector3 positionOnBlock)
    {
        entityId = 0;
        entityType = default;
        positionOnBlock = position;

        var gridPos = (GridPosition)position;
        if (!lineManager.TryGetPerspectiveLine(gridPos, out var perspectiveLine)) return false;

        // Find the highest visible block face (if any).
        var found = false;
        foreach (var zSet in perspectiveLine.ZFloors)
        {
            if (found) break;
            foreach (var info in zSet.Entities)
            {
                if (info.PerspectiveEntityType != PerspectiveEntityType.BlockFrontFace &&
                    info.PerspectiveEntityType != PerspectiveEntityType.BlockTopFace)
                    continue;

                if (overheadTransparency.GetOpacityForEntity(info.EntityId) < 1.0f)
                    continue;

                entityId = info.EntityId;
                entityType = info.PerspectiveEntityType;
                found = true;
                break;
            }
        }

        if (!found) return false;

        // Locate hovered position on the block face.
        if (!blockPositions.TryGetValue(entityId, out var blockPos))
        {
            logger.LogError("Found block {EntityId:X} on perspective line with no position.", entityId);
            return true;
        }

        var delta = entityType == PerspectiveEntityType.BlockTopFace
            ? gridPos.Z - blockPos.Z - 1.0f
            : gridPos.Y - position.Y;
        positionOnBlock = position with { Y = position.Y + delta, Z = position.Z - delta };
        return true;
    }

    public bool TryGetPerspectiveLine(GridPosition blockPosition,
        [NotNullWhen(true)] out PerspectiveLine? perspectiveLine)
    {
        return lineManager.TryGetPerspectiveLine(blockPosition, out perspectiveLine);
    }

    public float GetOpacityForEntity(ulong entityId)
    {
        return overheadTransparency.GetOpacityForEntity(entityId);
    }

    public int GetItemsUnderPlayer(Span<ulong> idBuffer)
    {
        if (idBuffer.Length == 0) return 0;
        if (!clientStateServices.TryGetSelectedPlayer(out var playerId))
        {
            logger.LogError("No player selected.");
            return 0;
        }

        if (!kinematics.TryGetValue(playerId, out var playerPosVel))
        {
            logger.LogError("Player has no position.");
            return 0;
        }

        var playerZf = (int)Math.Floor(playerPosVel.Position.Z);
        var count = 0;

        foreach (var (_, _, line) in lineManager.GetLineForEntity(playerId))
        {
            // Find z floor matching player.
            var idx = line.ZFloors.BinarySearch(EntityList.ForComparison(playerZf));
            if (idx < 0) continue;
            var entityList = line.ZFloors[idx];

            foreach (var itemId in entityList.Entities
                         .Where(info => Math.Abs(info.Z - playerPosVel.Position.Z) < ZTol &&
                                        info.PerspectiveEntityType == PerspectiveEntityType.NonBlock &&
                                        entityTypes.TryGetValue(info.EntityId, out var entityType) &&
                                        entityType == EntityType.Item)
                         .Select(info => info.EntityId))
            {
                idBuffer[count++] = itemId;
                if (count >= idBuffer.Length) return count;
            }
        }

        return count;
    }
}