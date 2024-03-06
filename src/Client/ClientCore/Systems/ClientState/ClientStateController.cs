// Sovereign Engine
// Copyright (c) 2024 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without eveClosen the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Sovereign.ClientCore.Events;
using Sovereign.EngineCore.Components.Indexers;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Public API for the ClientState system.
/// </summary>
public class ClientStateController
{
    /// <summary>
    ///     Informs the state system that the given world segment has been loaded.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="segmentIndex">World segment index.</param>
    public void WorldSegmentLoaded(IEventSender eventSender, GridPosition segmentIndex)
    {
        var details = new WorldSegmentEventDetails
        {
            SegmentIndex = segmentIndex
        };
        var ev = new Event(EventId.Client_State_WorldSegmentLoaded, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sets a state flag.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="flag">State flag.</param>
    /// <param name="newValue">New value.</param>
    public void SetStateFlag(IEventSender eventSender, ClientStateFlag flag, bool newValue)
    {
        var details = new ClientStateFlagEventDetails
        {
            Flag = flag,
            NewValue = newValue
        };
        var ev = new Event(EventId.Client_State_SetFlag, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sets the main menu state.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="state">New state.</param>
    public void SetMainMenuState(IEventSender eventSender, MainMenuState state)
    {
        var details = new MainMenuEventDetails
        {
            MainMenuState = state
        };
        var ev = new Event(EventId.Client_State_SetMainMenuState, details);
        eventSender.SendEvent(ev);
    }
}