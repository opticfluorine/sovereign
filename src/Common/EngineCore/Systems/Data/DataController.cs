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

using System;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;
using EventId = Sovereign.EngineCore.Events.EventId;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Controller interface for the Data system.
/// </summary>
public interface IDataController
{
    /// <summary>
    ///     Sets a global key-value pair.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to string.</exception>
    void SetGlobal<T>(IEventSender eventSender, string key, T value) where T : notnull;

    /// <summary>
    ///     Removes a global key-value pair.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="key">Key.</param>
    void RemoveGlobal(IEventSender eventSender, string key);
}

/// <summary>
///     Implementation of the IDataController interface.
/// </summary>
internal class DataController : IDataController
{
    public void SetGlobal<T>(IEventSender eventSender, string key, T value) where T : notnull
    {
        var details = new KeyValueEventDetails
        {
            Key = key,
            Value = value.ToString() ?? throw new InvalidCastException()
        };
        var ev = new Event(EventId.Core_Data_SetGlobal, details);
        eventSender.SendEvent(ev);
    }

    public void RemoveGlobal(IEventSender eventSender, string key)
    {
        var details = new StringEventDetails
        {
            Value = key
        };
        var ev = new Event(EventId.Core_Data_RemoveGlobal, details);
        eventSender.SendEvent(ev);
    }
}