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
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.World;

namespace Sovereign.EngineCore.Systems.Block;

/// <summary>
///     Block presence grid for a single z plane in a world segment.
/// </summary>
public class BlockPresenceGrid
{
    /// <summary>
    ///     Number of blocks in the grid.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     Block presence grid, indexed in row-major order (index[x,y] = y * SegmentLength + x).
    ///     true indicates a block is present, false indicates a block is absent.
    /// </summary>
    public bool[] Grid { get; set; } =
        new bool[WorldConstants.SegmentLength * WorldConstants.SegmentLength];
}

/// <summary>
///     Tracks the placement of blocks into planar grids by world segment and depth.
/// </summary>
public class BlockGridTracker
{
    /// <summary>
    ///     Block presence grids indexed by world segment index and Z depth.
    /// </summary>
    private readonly ConcurrentDictionary<Tuple<GridPosition, int>, BlockPresenceGrid> grids = new();

    private readonly BlockInternalController internalController;

    private readonly WorldSegmentResolver resolver;

    public BlockGridTracker(WorldSegmentResolver resolver, BlockInternalController internalController)
    {
        this.resolver = resolver;
        this.internalController = internalController;
    }

    /// <summary>
    ///     Gets the block presence grid for the given world segment and Z plane if it exists.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="z">Z.</param>
    /// <param name="grid">Grid. Only valid if this method returns true.</param>
    /// <returns>Block presence grid.</returns>
    public bool TryGetGrid(GridPosition segmentIndex, int z, [NotNullWhen(true)] out BlockPresenceGrid? grid)
    {
        return grids.TryGetValue(Tuple.Create(segmentIndex, z), out grid);
    }

    /// <summary>
    ///     Tracks a block which has been added (or loaded).
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    public void AddBlock(GridPosition blockPosition)
    {
        var segmentIndex = resolver.GetWorldSegmentForPosition(blockPosition);
        var segmentBasePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var gridKey = Tuple.Create(segmentIndex, blockPosition.Z);

        if (!grids.TryGetValue(gridKey, out var grid))
        {
            grid = new BlockPresenceGrid();
            grids[gridKey] = grid;
        }

        var relX = blockPosition.X - (int)segmentBasePos.X;
        var relY = blockPosition.Y - (int)segmentBasePos.Y;
        var index = WorldConstants.SegmentLength * relY + relX;
        grid.Grid[index] = true;
        grid.Count++;

        internalController.AnnounceBlockPresenceGridUpdated(segmentIndex, blockPosition.Z);
    }

    /// <summary>
    ///     Stops tracking a block which has been removed (or unloaded).
    /// </summary>
    /// <param name="blockPosition">Block position.</param>
    public void RemoveBlock(GridPosition blockPosition)
    {
        var segmentIndex = resolver.GetWorldSegmentForPosition(blockPosition);
        var segmentBasePos = resolver.GetRangeForWorldSegment(segmentIndex).Item1;
        var gridKey = Tuple.Create(segmentIndex, blockPosition.Z);

        if (!grids.TryGetValue(gridKey, out var grid)) return;

        var relX = blockPosition.X - (int)segmentBasePos.X;
        var relY = blockPosition.Y - (int)segmentBasePos.Y;
        var index = WorldConstants.SegmentLength * relY + relX;
        grid.Grid[index] = false;
        grid.Count--;

        internalController.AnnounceBlockPresenceGridUpdated(segmentIndex, blockPosition.Z);
    }
}