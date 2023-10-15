// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
using Sovereign.ServerCore.Events;

namespace Sovereign.ServerCore.Systems.Debug;

/// <summary>
///     Controller providing a public API for the debug system.
/// </summary>
public class DebugController
{
    /// <summary>
    ///     Issues a debug command.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="command">Debug command to send.</param>
    public void SendDebugCommand(IEventSender eventSender, DebugCommand command)
    {
        var ev = new Event(EventId.Server_Debug_Command, new DebugCommandEventDetails
        {
            Command = command
        });
        eventSender.SendEvent(ev);
    }
}