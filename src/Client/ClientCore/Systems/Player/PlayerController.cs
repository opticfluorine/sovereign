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

namespace Sovereign.ClientCore.Systems.Player;

/// <summary>
///     Controller class for Player system.
/// </summary>
public sealed class PlayerController
{
    /// <summary>
    ///     Picks up an item under the player.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void PickUpItemUnder(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Player_PickUpItemUnder);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Attempts to interact with an entity in front of the player.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void Interact(IEventSender eventSender)
    {
        var ev = new Event(EventId.Client_Player_Interact);
        eventSender.SendEvent(ev);
    }
}