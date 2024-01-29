// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

namespace Sovereign.Persistence.Systems.Persistence;

/// <summary>
///     Internal controller API for Persistence system.
/// </summary>
public class PersistenceInternalController
{
    /// <summary>
    ///     Announces that synchronization is complete.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void CompleteSync(IEventSender eventSender)
    {
        var ev = new Event(EventId.Server_Persistence_SynchronizeComplete);
        eventSender.SendEvent(ev);
    }
}