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
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Holds the in-memory cache of world segment key-value data for the loaded world segments.
/// </summary>
/// <remarks>
///     Segments with no key-value pairs have no value entry, so an absent entry alone does not
///     indicate an unloaded segment. Loaded segments are therefore tracked separately to let
///     readers distinguish an unloaded segment (safe default: absence, with a warning) from a
///     loaded segment that simply has no pairs.
/// </remarks>
public sealed class SegmentKeyValueManager
{
    /// <summary>
    ///     World segments whose key-value data is currently loaded into the cache.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, byte> loadedSegments = new();

    /// <summary>
    ///     Key-value pairs for the loaded world segments. Segments with no pairs are absent.
    /// </summary>
    private readonly ConcurrentDictionary<GridPosition, ConcurrentDictionary<string, DataValue>> valuesBySegment = new();

    /// <summary>
    ///     Checks whether the given world segment's key-value data is loaded into the cache.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>true if the segment is loaded, false otherwise.</returns>
    public bool IsLoaded(GridPosition segment)
    {
        return loadedSegments.ContainsKey(segment);
    }

    /// <summary>
    ///     Replaces the cached key-value pairs for the given world segment with a freshly loaded set.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="pairs">Loaded key-value pairs, possibly empty.</param>
    public void LoadSegment(GridPosition segment, IReadOnlyDictionary<string, DataValue> pairs)
    {
        loadedSegments[segment] = 0;

        if (pairs.Count == 0)
            valuesBySegment.TryRemove(segment, out _);
        else
            valuesBySegment[segment] = new ConcurrentDictionary<string, DataValue>(pairs);
    }

    /// <summary>
    ///     Gets the value stored for the given key of the given world segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Stored value. Only valid if the method returns true.</param>
    /// <returns>true if the value exists, false otherwise.</returns>
    public bool TryGetValue(GridPosition segment, string key, out DataValue value)
    {
        value = default;

        return valuesBySegment.TryGetValue(segment, out var values) && values.TryGetValue(key, out value);
    }

    /// <summary>
    ///     Stores a key-value pair for the given world segment, creating the segment entry lazily.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value to store.</param>
    public void SetValue(GridPosition segment, string key, DataValue value)
    {
        loadedSegments[segment] = 0;

        var values = valuesBySegment.GetOrAdd(segment, _ => new ConcurrentDictionary<string, DataValue>());
        values[key] = value;
    }

    /// <summary>
    ///     Removes the value stored for the given key of the given world segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    public void RemoveKey(GridPosition segment, string key)
    {
        loadedSegments[segment] = 0;

        if (valuesBySegment.TryGetValue(segment, out var values))
        {
            values.TryRemove(key, out _);
            if (values.IsEmpty) valuesBySegment.TryRemove(segment, out _);
        }
    }

    /// <summary>
    ///     Drops the cached key-value data for the given world segment on deactivation.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    public void DropSegment(GridPosition segment)
    {
        loadedSegments.TryRemove(segment, out _);
        valuesBySegment.TryRemove(segment, out _);
    }

    /// <summary>
    ///     Copies every cached key-value pair of the given world segment into the given dictionary.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="pairs">Dictionary to populate.</param>
    public void GetAllKeyValuePairs(GridPosition segment, Dictionary<string, DataValue> pairs)
    {
        if (!valuesBySegment.TryGetValue(segment, out var values)) return;

        foreach (var (key, value) in values) pairs[key] = value;
    }
}
