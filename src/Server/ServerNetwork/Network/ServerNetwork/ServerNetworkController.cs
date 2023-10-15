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

namespace Sovereign.ServerNetwork.Network.ServerNetwork;

/// <summary>
///     Provides the public API for the server network system.
/// </summary>
public class ServerNetworkController
{
    /// <summary>
    ///     Requests that the client with the given connection ID be disconnected from the server.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="connectionId">Connection ID.</param>
    public void Disconnect(IEventSender eventSender, int connectionId)
    {
        var ev = new Event(EventId.Server_Network_DisconnectClient,
            new ConnectionIdEventDetails { ConnectionId = connectionId });
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Announces that the client associated with the given connection ID has disconnected.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="connectionId">Connection ID.</param>
    public void ClientDisconnected(IEventSender eventSender, int connectionId)
    {
        var ev = new Event(EventId.Server_Network_ClientDisconnected,
            new ConnectionIdEventDetails { ConnectionId = connectionId });
        eventSender.SendEvent(ev);
    }
}