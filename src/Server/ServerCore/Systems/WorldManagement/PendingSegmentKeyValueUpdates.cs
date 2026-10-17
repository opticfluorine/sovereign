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
///     A pending set or removal of a single world segment key-value pair.
/// </summary>
public readonly struct PendingSegmentKeyValueUpdate
{
    /// <summary>
    ///     Affected world segment.
    /// </summary>
    public GridPosition Segment { get; init; }

    /// <summary>
    ///     Affected key.
    /// </summary>
    public string Key { get; init; }

    /// <summary>
    ///     Value to store, or null to remove the key.
    /// </summary>
    public DataValue? Value { get; init; }
}

/// <summary>
///     Holds the world segment key-value updates that have been applied to the cache but not yet
///     written to the database.
/// </summary>
/// <remarks>
///     Entries are appended by writers on any thread and drained or folded on the persistence
///     thread, so a concurrent queue is used. Pending updates are kept until the persister drains
///     them; this lets a segment that deactivates and reactivates before the next synchronization
///     fold its still-pending updates over the freshly loaded database rows instead of losing them.
/// </remarks>
public sealed class PendingSegmentKeyValueUpdates
{
    /// <summary>
    ///     Pending updates, in the order they were received.
    /// </summary>
    private readonly ConcurrentQueue<PendingSegmentKeyValueUpdate> pending = new();

    /// <summary>
    ///     Queues a key-value update, or a removal when the value is null.
    /// </summary>
    /// <param name="segment">Affected world segment.</param>
    /// <param name="key">Affected key.</param>
    /// <param name="value">Value to store, or null to remove the key.</param>
    public void Enqueue(GridPosition segment, string key, DataValue? value)
    {
        pending.Enqueue(new PendingSegmentKeyValueUpdate
        {
            Segment = segment,
            Key = key,
            Value = value
        });
    }

    /// <summary>
    ///     Folds every still-pending update for the given segment over the given loaded pairs.
    /// </summary>
    /// <param name="segment">World segment index.</param>
    /// <param name="pairs">Loaded key-value pairs to fold the pending updates into.</param>
    public void FoldPending(GridPosition segment, Dictionary<string, DataValue> pairs)
    {
        foreach (var update in pending)
        {
            if (update.Segment != segment) continue;

            if (update.Value.HasValue)
                pairs[update.Key] = update.Value.Value;
            else
                pairs.Remove(update.Key);
        }
    }

    /// <summary>
    ///     Removes and returns all pending updates.
    /// </summary>
    /// <returns>Pending updates in the order they were received.</returns>
    public IReadOnlyList<PendingSegmentKeyValueUpdate> TakeAll()
    {
        var result = new List<PendingSegmentKeyValueUpdate>();
        while (pending.TryDequeue(out var update)) result.Add(update);
        return result;
    }
}
