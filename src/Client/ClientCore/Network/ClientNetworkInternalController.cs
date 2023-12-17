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
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Network;

/// <summary>
///     Internal API for sending events from the client network system.
/// </summary>
public class ClientNetworkInternalController
{
    /// <summary>
    ///     Sets the entity ID of the current active player.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="playerEntityId">Player entity ID.</param>
    public void SetPlayerEntityId(IEventSender eventSender, ulong playerEntityId)
    {
        var details = new EntityEventDetails
        {
            EntityId = playerEntityId
        };
        eventSender.SendEvent(new Event(EventId.Client_Network_PlayerEntitySelected, details));
    }
}