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
using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Writes pending world segment flag updates to the database.
/// </summary>
public sealed class SegmentFlagPersister
{
    private readonly PendingSegmentFlagUpdates pending;

    public SegmentFlagPersister(PendingSegmentFlagUpdates pending)
    {
        this.pending = pending;
    }

    /// <summary>
    ///     Drains the pending flag updates and applies them to the database within the given
    ///     transaction.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    public void SynchronizeSegmentFlags(IPersistenceProvider provider, IDbTransaction transaction)
    {
        var updates = pending.TakeAll();
        if (updates.Count == 0) return;

        var getInRange = provider.GetSegmentFlagsInRangeQuery;
        var setFlags = provider.SetSegmentFlagsQuery;

        foreach (var update in updates)
        {
            // Read the currently stored rows over the box so that repeated updates are idempotent.
            var stored = getInRange.GetFlagsInRange(update.MinSegment, update.MaxSegment, transaction);

            for (var x = update.MinSegment.X; x <= update.MaxSegment.X; ++x)
                for (var y = update.MinSegment.Y; y <= update.MaxSegment.Y; ++y)
                    for (var z = update.MinSegment.Z; z <= update.MaxSegment.Z; ++z)
                    {
                        var segment = new GridPosition(x, y, z);
                        var storedFlags = stored.TryGetValue(segment, out var value) ? value : WorldSegmentFlag.None;
                        var updatedFlags = update.Set ? storedFlags | update.Flag : storedFlags & ~update.Flag;

                        // Skip rows that remain absent so an empty box does not fan out delete statements.
                        if (updatedFlags == WorldSegmentFlag.None && !stored.ContainsKey(segment)) continue;

                        setFlags.SetFlags(segment, updatedFlags, transaction);
                    }
        }
    }
}
