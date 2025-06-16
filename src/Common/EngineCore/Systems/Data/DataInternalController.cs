// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Internal controller for Data system.
/// </summary>
internal class DataInternalController
{
    private readonly IEventSender eventSender;

    public DataInternalController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Announces a global key-value pair has been added or updated.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void GlobalSet(string key, string value)
    {
        var details = new KeyValueEventDetails
        {
            Key = key,
            Value = value
        };
        var ev = new Event(EventId.Core_Data_GlobalSet, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces a global key-value pair removal.
    /// </summary>
    /// <param name="key">Key.</param>
    public void GlobalRemoved(string key)
    {
        var details = new StringEventDetails
        {
            Value = key
        };
        var ev = new Event(EventId.Core_Data_GlobalRemoved, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces an entity key-value pair update.
    /// </summary>
    /// <param name="localEventSender">Thread-local event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void EntityKeyValueSet(IEventSender localEventSender, ulong entityId, string key, string value)
    {
        var details = new EntityKeyValueEventDetails
        {
            EntityId = entityId,
            Key = key,
            Value = value
        };
        var ev = new Event(EventId.Core_Data_EntityKeyValueSet, details);
        localEventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces an entity key-value pair removal.
    /// </summary>
    /// <param name="localEventSender">Thread-local event sender.</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    public void EntityKeyValueRemoved(IEventSender localEventSender, ulong entityId, string key)
    {
        var details = new EntityStringEventDetails
        {
            EntityId = entityId,
            Value = key
        };
        var ev = new Event(EventId.Core_Data_EntityKeyValueRemoved, details);
        localEventSender.SendEvent(ev);
    }
}