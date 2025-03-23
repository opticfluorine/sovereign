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
using System.Linq;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Provides a read API to the block system.
/// </summary>
public interface IBlockServices
{
    /// <summary>
    ///     Checks whether the given entity is a block.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">Whether to use lookback for very recently deleted components.</param>
    /// <returns>true if the entity is a block, false otherwise.</returns>
    bool IsEntityBlock(ulong entityId, bool lookback = false);

    /// <summary>
    ///     Checks whether a block exists at the given position.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <returns>true if a block exists, false otherwise.</returns>
    bool BlockExistsAtPosition(GridPosition blockPosition);

    /// <summary>
    ///     Gets the entity ID of the block at the given position if one exists.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <param name="entityId">Set to the block entity ID, or 0 if none exists.</param>
    /// <returns>true if a block exists at the given position, false otherwise.</returns>
    bool TryGetBlockAtPosition(GridPosition blockPosition, out ulong entityId);

    /// <summary>
    ///     Gets the block presence grid (if any) for the given world segment and z plane.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z.</param>
    /// <param name="grid">Block presence grid.</param>
    /// <returns>true if a grid was found, false otherwise (implies no blocks present).</returns>
    bool TryGetBlockPresenceGrid(GridPosition segmentIndex, int z,
        [NotNullWhen(true)] out BlockPresenceGrid? grid);
}

/// <summary>
///     Implementation of IBlockServices.
/// </summary>
public class BlockServices : IBlockServices
{
    private readonly BlockGridPositionIndexer blockGridPositionIndexer;
    private readonly BlockGridTracker blockGridTracker;
    private readonly MaterialComponentCollection materials;

    public BlockServices(MaterialComponentCollection materials, BlockGridPositionIndexer blockGridPositionIndexer,
        BlockGridTracker blockGridTracker)
    {
        this.materials = materials;
        this.blockGridPositionIndexer = blockGridPositionIndexer;
        this.blockGridTracker = blockGridTracker;
    }

    public bool IsEntityBlock(ulong entityId, bool lookback = false)
    {
        return materials.HasComponentForEntity(entityId, lookback) || materials.HasPendingComponentForEntity(entityId);
    }

    public bool BlockExistsAtPosition(GridPosition blockPosition)
    {
        return blockGridPositionIndexer.GetEntitiesAtPosition(blockPosition) != null;
    }

    public bool TryGetBlockAtPosition(GridPosition blockPosition, out ulong entityId)
    {
        entityId = 0;
        var entities = blockGridPositionIndexer.GetEntitiesAtPosition(blockPosition);
        if (entities is { Count: > 0 })
        {
            entityId = entities.Keys.First();
            return true;
        }

        return false;
    }

    public bool TryGetBlockPresenceGrid(GridPosition segmentIndex, int z,
        [NotNullWhen(true)] out BlockPresenceGrid? grid)
    {
        return blockGridTracker.TryGetGrid(segmentIndex, z, out grid);
    }
}