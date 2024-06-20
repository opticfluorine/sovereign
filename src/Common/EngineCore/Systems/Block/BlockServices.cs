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

using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Provides a read API to the block system.
/// </summary>
public class BlockServices
{
    private readonly BlockGridPositionIndexer blockGridPositionIndexer;
    private readonly MaterialComponentCollection materials;

    public BlockServices(MaterialComponentCollection materials, BlockGridPositionIndexer blockGridPositionIndexer)
    {
        this.materials = materials;
        this.blockGridPositionIndexer = blockGridPositionIndexer;
    }

    /// <summary>
    ///     Checks whether the given entity is a block.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">Whether to use lookback for very recently deleted components.</param>
    /// <returns>true if the entity is a block, false otherwise.</returns>
    public bool IsEntityBlock(ulong entityId, bool lookback = false)
    {
        return materials.HasComponentForEntity(entityId, lookback);
    }

    /// <summary>
    ///     Checks whether a block exists at the given position.
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    /// <returns>true if a block exists, false otherwise.</returns>
    public bool BlockExistsAtPosition(GridPosition blockPosition)
    {
        return blockGridPositionIndexer.GetEntitiesAtPosition(blockPosition) != null;
    }
}