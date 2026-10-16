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

using System;
using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Main;
using Sovereign.EngineCore.Systems.Data;
using Sovereign.Persistence.Database;
using Sovereign.ServerCore.Systems.WorldManagement;

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Loads a world segment's key-value data from the database into the gameplay cache.
/// </summary>
public sealed class SegmentKeyValueRetriever
{
    private readonly SegmentKeyValueManager manager;
    private readonly PendingSegmentKeyValueUpdates pending;

    public SegmentKeyValueRetriever(SegmentKeyValueManager manager, PendingSegmentKeyValueUpdates pending)
    {
        this.manager = manager;
        this.pending = pending;
    }

    /// <summary>
    ///     Loads the stored key-value pairs for the given world segment and folds any pending
    ///     updates into them before caching, so the segment never caches a stale value.
    /// </summary>
    /// <param name="provider">Persistence provider.</param>
    /// <param name="segmentIndex">World segment index.</param>
    /// <exception cref="FatalErrorException">Thrown if a stored value cannot be decoded.</exception>
    public void RetrieveKeyValues(IPersistenceProvider provider, GridPosition segmentIndex)
    {
        var pairs = new Dictionary<string, DataValue>();

        foreach (var row in provider.GetSegmentKeyValuesQuery.GetKeyValues(segmentIndex))
            try
            {
                var type = (DataValueType)row.Type;
                pairs[row.Key] = DataValueCodec.Decode(type, row.Value);
            }
            catch (FormatException e)
            {
                throw new FatalErrorException(
                    $"Malformed world segment key-value data for segment {segmentIndex}, key '{row.Key}' (type {row.Type}).",
                    e);
            }

        pending.FoldPending(segmentIndex, pairs);
        manager.LoadSegment(segmentIndex, pairs);
    }
}
