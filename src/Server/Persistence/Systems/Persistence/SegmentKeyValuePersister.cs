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

using System.Data;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Writes pending world segment key-value updates to the database.
/// </summary>
public sealed class SegmentKeyValuePersister
{
    private readonly PendingSegmentKeyValueUpdates pending;

    public SegmentKeyValuePersister(PendingSegmentKeyValueUpdates pending)
    {
        this.pending = pending;
    }

    /// <summary>
    ///     Drains the pending key-value updates and applies them to the database within the given
    ///     transaction.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    public void SynchronizeSegmentKeyValues(IPersistenceProvider provider, IDbTransaction transaction)
    {
        var updates = pending.TakeAll();
        if (updates.Count == 0) return;

        var setKeyValue = provider.SetSegmentKeyValueQuery;
        var removeKeyValue = provider.RemoveSegmentKeyValueQuery;

        foreach (var update in updates)
            if (update.Value.HasValue)
                setKeyValue.SetKeyValue(update.Segment, update.Key, update.Value.Value, transaction);
            else
                removeKeyValue.RemoveKeyValue(update.Segment, update.Key, transaction);
    }
}
