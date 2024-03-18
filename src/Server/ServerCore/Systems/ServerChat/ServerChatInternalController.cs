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

using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.ServerChat;

/// <summary>
///     Internal controller for ServerChat system.
/// </summary>
public class ServerChatInternalController
{
    private readonly IEventSender eventSender;

    public ServerChatInternalController(IEventSender eventSender)
    {
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Sends a local chat message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="senderEntityId">Entity ID of sending entity.</param>
    /// <param name="segmentIndex">World segment index of origination.</param>
    public void SendLocalChat(string message, ulong senderEntityId, GridPosition segmentIndex)
    {
        var details = new LocalChatEventDetails
        {
            Message = message,
            SegmentIndex = segmentIndex,
            SenderEntityId = senderEntityId
        };
        var ev = new Event(EventId.Core_Chat_Local, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends a global chat message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="senderName">Sender name.</param>
    public void SendGlobalChat(string message, string senderName)
    {
        var details = new GlobalChatEventDetails
        {
            Message = message,
            SenderName = senderName
        };
        var ev = new Event(EventId.Core_Chat_Global, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sends a system message to the given player entity ID.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="targetEntityId">Target player entity ID.</param>
    public void SendSystemMessage(string message, ulong targetEntityId)
    {
        var details = new SystemChatEventDetails
        {
            Message = message,
            TargetEntityId = targetEntityId
        };
        var ev = new Event(EventId.Core_Chat_System, details);
        eventSender.SendEvent(ev);
    }
}