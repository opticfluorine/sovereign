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
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Write facade for world segment key-value data.
/// </summary>
/// <remarks>
///     Writes mutate the cache immediately and enqueue a delta for the persistence writer, so a
///     write is visible to a following read without waiting for a database round trip. Keys that
///     begin with two underscores are reserved and rejected.
/// </remarks>
public sealed class SegmentKeyValueController : ISegmentKeyValueController
{
    private readonly IDataServices dataServices;
    private readonly SegmentKeyValueManager manager;
    private readonly PendingSegmentKeyValueUpdates pending;

    public SegmentKeyValueController(SegmentKeyValueManager manager, PendingSegmentKeyValueUpdates pending,
        IDataServices dataServices)
    {
        this.manager = manager;
        this.pending = pending;
        this.dataServices = dataServices;
    }

    public bool IsSegmentLoaded(GridPosition segment)
    {
        return manager.IsLoaded(segment);
    }

    public bool TryGetValue(GridPosition segment, string key, out DataValue value)
    {
        return manager.TryGetValue(segment, key, out value);
    }

    public bool SetValue(GridPosition segment, string key, DataValue value)
    {
        if (dataServices.IsKeyReadOnly(key)) return false;

        manager.SetValue(segment, key, value);
        pending.Enqueue(segment, key, value);
        return true;
    }

    public bool RemoveKey(GridPosition segment, string key)
    {
        if (dataServices.IsKeyReadOnly(key)) return false;

        manager.RemoveKey(segment, key);
        pending.Enqueue(segment, key, null);
        return true;
    }
}
