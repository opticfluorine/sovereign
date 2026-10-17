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
using Sovereign.EngineCore.Systems.Data;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.ServerCore.TestSupport;

/// <summary>
///     Test double for ISegmentKeyValueController that records writes and serves seeded reads.
/// </summary>
public sealed class RecordingSegmentKeyValueController : ISegmentKeyValueController
{
    private readonly HashSet<GridPosition> loadedSegments = new();
    private readonly Dictionary<GridPosition, Dictionary<string, DataValue>> values = new();

    /// <summary>
    ///     Set operations recorded, in order.
    /// </summary>
    public List<(GridPosition Segment, string Key, DataValue Value)> SetValues { get; } = new();

    /// <summary>
    ///     Remove operations recorded, in order.
    /// </summary>
    public List<(GridPosition Segment, string Key)> RemovedKeys { get; } = new();

    public bool IsSegmentLoaded(GridPosition segment)
    {
        return loadedSegments.Contains(segment);
    }

    public bool TryGetValue(GridPosition segment, string key, out DataValue value)
    {
        value = default;
        return values.TryGetValue(segment, out var segmentValues) && segmentValues.TryGetValue(key, out value);
    }

    public bool SetValue(GridPosition segment, string key, DataValue value)
    {
        if (key.StartsWith("__", System.StringComparison.Ordinal)) return false;

        loadedSegments.Add(segment);
        GetSegmentValues(segment)[key] = value;
        SetValues.Add((segment, key, value));
        return true;
    }

    public bool RemoveKey(GridPosition segment, string key)
    {
        if (key.StartsWith("__", System.StringComparison.Ordinal)) return false;

        loadedSegments.Add(segment);
        if (values.TryGetValue(segment, out var segmentValues)) segmentValues.Remove(key);
        RemovedKeys.Add((segment, key));
        return true;
    }

    /// <summary>
    ///     Marks a segment as loaded without recording a write.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    public void MarkLoaded(GridPosition segment)
    {
        loadedSegments.Add(segment);
    }

    /// <summary>
    ///     Seeds a readable value without recording a write.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void Seed(GridPosition segment, string key, DataValue value)
    {
        loadedSegments.Add(segment);
        GetSegmentValues(segment)[key] = value;
    }

    /// <summary>
    ///     Gets or creates the value dictionary for a segment.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <returns>Value dictionary.</returns>
    private Dictionary<string, DataValue> GetSegmentValues(GridPosition segment)
    {
        if (!values.TryGetValue(segment, out var segmentValues))
        {
            segmentValues = new Dictionary<string, DataValue>();
            values[segment] = segmentValues;
        }

        return segmentValues;
    }
}
