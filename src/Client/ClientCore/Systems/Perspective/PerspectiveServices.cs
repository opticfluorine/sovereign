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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Sovereign.ClientCore.Rendering.Configuration;
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
}

/// <summary>
///     Implementation of IPerspectiveServices.
/// </summary>
internal class PerspectiveServices : IPerspectiveServices
{
    private readonly PerspectiveLineManager lineManager;
    private readonly OverheadTransparency overheadTransparency;
    private readonly BlockPositionComponentCollection blockPositions;
    private readonly ILogger<PerspectiveServices> logger;
    private readonly DisplayViewport viewport;

    public PerspectiveServices(PerspectiveLineManager lineManager, DisplayViewport viewport,
        OverheadTransparency overheadTransparency, BlockPositionComponentCollection blockPositions,
        ILogger<PerspectiveServices> logger)
    {
        this.lineManager = lineManager;
        this.viewport = viewport;
        this.overheadTransparency = overheadTransparency;
        this.blockPositions = blockPositions;
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

        if (!found) return false;

        // Locate hovered position on the block face.
        if (!blockPositions.TryGetValue(entityId, out var blockPos))
        {
            logger.LogError("Found block {EntityId:X} on perspective line with no position.", entityId);
            return true;
        }
        var delta = entityType == PerspectiveEntityType.BlockTopFace
            ? position.Z - gridPos.Z - 1.0f
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
}