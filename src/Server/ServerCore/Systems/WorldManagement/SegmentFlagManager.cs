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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Holds the in-memory cache of world segment flags used by gameplay.
/// </summary>
public sealed class SegmentFlagManager
{
    /// <summary>
    ///     Flags for the loaded world segments. Segments with no flags are absent from the map.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, WorldSegmentFlag> flagsBySegment = new();

    /// <summary>
    ///     Gets the flags for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Flags for the segment, or None if the segment has no flags.</returns>
    public WorldSegmentFlag GetFlags(GridPosition segmentIndex)
    {
        return flagsBySegment.TryGetValue(segmentIndex, out var flags) ? flags : WorldSegmentFlag.None;
    }

    /// <summary>
    ///     Sets the flags for the given world segment, removing the entry when no flags remain.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="flags">Flags to store.</param>
    public void SetFlagsInCache(GridPosition segmentIndex, WorldSegmentFlag flags)
    {
        if (flags == WorldSegmentFlag.None)
            flagsBySegment.TryRemove(segmentIndex, out _);
        else
            flagsBySegment[segmentIndex] = flags;
    }

    /// <summary>
    ///     Enumerates the flagged world segments within the given inclusive box.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the box.</param>
    /// <param name="max">Maximum (inclusive) corner of the box.</param>
    /// <returns>Flagged segments within the box.</returns>
    public IEnumerable<(GridPosition segment, WorldSegmentFlag flags)> GetFlagsInBox(GridPosition min, GridPosition max)
    {
        foreach (var (segment, flags) in flagsBySegment)
            if (segment.X >= min.X && segment.X <= max.X
                && segment.Y >= min.Y && segment.Y <= max.Y
                && segment.Z >= min.Z && segment.Z <= max.Z)
                yield return (segment, flags);
    }
}
