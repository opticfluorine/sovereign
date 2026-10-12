// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     A pending set or clear of a world segment flag over an inclusive box of world segments.
/// </summary>
public readonly struct PendingSegmentFlagUpdate
{
    /// <summary>
    ///     Minimum (inclusive) corner of the affected segment box.
    /// </summary>
    public GridPosition MinSegment { get; init; }

    /// <summary>
    ///     Maximum (inclusive) corner of the affected segment box.
    /// </summary>
    public GridPosition MaxSegment { get; init; }

    /// <summary>
    ///     Flag to apply.
    /// </summary>
    public WorldSegmentFlag Flag { get; init; }

    /// <summary>
    ///     Whether to set the flag (true) or clear it (false).
    /// </summary>
    public bool Set { get; init; }
}

/// <summary>
///     Holds the world segment flag updates that have been applied to the cache but not yet
///     written to the database.
/// </summary>
/// <remarks>
///     This type is not thread-safe. It is only accessed by the persistence system's executor
///     thread, so no locking is required.
/// </remarks>
public sealed class PendingSegmentFlagUpdates
{
    /// <summary>
    ///     Pending updates, in the order they were received.
    /// </summary>
    private readonly List<PendingSegmentFlagUpdate> pending = new();

    /// <summary>
    ///     Queues a raw flag update.
    /// </summary>
    /// <param name="minSegment">Minimum (inclusive) corner of the affected segment box.</param>
    /// <param name="maxSegment">Maximum (inclusive) corner of the affected segment box.</param>
    /// <param name="flag">Flag to apply.</param>
    /// <param name="set">Whether to set the flag (true) or clear it (false).</param>
    public void Enqueue(GridPosition minSegment, GridPosition maxSegment, WorldSegmentFlag flag, bool set)
    {
        pending.Add(new PendingSegmentFlagUpdate
        {
            MinSegment = minSegment,
            MaxSegment = maxSegment,
            Flag = flag,
            Set = set
        });
    }

    /// <summary>
    ///     Folds every pending update that touches the given segment into the given flag value.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="flags">Flags read before applying the pending updates.</param>
    /// <returns>Flags with all pending updates for the segment applied.</returns>
    public WorldSegmentFlag FoldPending(GridPosition segment, WorldSegmentFlag flags)
    {
        foreach (var update in pending)
        {
            if (segment.X < update.MinSegment.X || segment.X > update.MaxSegment.X) continue;
            if (segment.Y < update.MinSegment.Y || segment.Y > update.MaxSegment.Y) continue;
            if (segment.Z < update.MinSegment.Z || segment.Z > update.MaxSegment.Z) continue;

            flags = update.Set ? flags | update.Flag : flags & ~update.Flag;
        }

        return flags;
    }

    /// <summary>
    ///     Removes and returns all pending updates.
    /// </summary>
    /// <returns>Pending updates in the order they were received.</returns>
    public IReadOnlyList<PendingSegmentFlagUpdate> TakeAll()
    {
        var result = pending.ToArray();
        pending.Clear();
        return result;
    }
}
