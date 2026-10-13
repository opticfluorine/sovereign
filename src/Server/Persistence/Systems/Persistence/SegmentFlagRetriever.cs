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

using Sovereign.EngineCore.Components.Types;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Loads a world segment's flags from the database into the gameplay cache.
/// </summary>
public sealed class SegmentFlagRetriever
{
    private readonly PendingSegmentFlagUpdates pending;
    private readonly SegmentFlagManager segmentFlagManager;

    public SegmentFlagRetriever(SegmentFlagManager segmentFlagManager, PendingSegmentFlagUpdates pending)
    {
        this.segmentFlagManager = segmentFlagManager;
        this.pending = pending;
    }

    /// <summary>
    ///     Loads the stored flags for the given world segment and folds any pending updates into
    ///     them before caching, so the segment never caches a stale value.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void RetrieveFlags(IPersistenceProvider provider, GridPosition segmentIndex)
    {
        var flags = provider.GetSegmentFlagsQuery.GetFlags(segmentIndex);
        flags = pending.FoldPending(segmentIndex, flags);
        segmentFlagManager.SetFlagsInCache(segmentIndex, flags);
    }
}
