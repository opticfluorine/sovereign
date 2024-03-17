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
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientChat;

/// <summary>
///     Public API for client-side chat operations.
/// </summary>
public class ClientChatController
{
    /// <summary>
    ///     Sends a chat message.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="message">Message.</param>
    public void SendChat(IEventSender eventSender, string message)
    {
        // No need to fill in the sender, the server will do this automatically.
        var details = new ChatEventDetails
        {
            Message = message
        };

        // Send.
        var ev = new Event(EventId.Core_Chat_Send, details);
        eventSender.SendEvent(ev);
    }
}