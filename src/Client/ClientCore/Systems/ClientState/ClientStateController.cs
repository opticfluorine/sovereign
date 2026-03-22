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

using Sovereign.ClientCore.Events.Details;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Public API for the ClientState system.
/// </summary>
public class ClientStateController(HighlightManager highlightManager)
{
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

    /// <summary>
    ///     Selects an item from the inventory for GUI operations.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="slotIndex">Inventory slot index.</param>
    public void SelectItem(IEventSender eventSender, int slotIndex)
    {
        var details = new IntEventDetails { Value = (uint)slotIndex };
        var ev = new Event(EventId.Client_State_SelectItem, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Deselects any currently selected item for GUI operations.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    public void DeselectItem(IEventSender eventSender)
    {
        SelectItem(eventSender, -1);
    }

    /// <summary>
    ///     Synchronously clears any block highlights.
    /// </summary>
    public void ClearBlockHighlights()
    {
        highlightManager.ClearBlockHighlight();
    }

    /// <summary>
    ///     Synchronously adds a block highlight.
    /// </summary>
    /// <param name="position">Position.</param>
    public void AddBlockHighlight(GridPosition position)
    {
        highlightManager.AddBlockHighlight(position);
    }

    /// <summary>
    ///     Synchronously adds block highlights to a rectangle in the XY plane.
    /// </summary>
    /// <param name="bottomLeft">Bottom left corner of rectangle.</param>
    /// <param name="width">Width in blocks.</param>
    /// <param name="height">Height in blocks.</param>
    public void AddBlockHighlightXyRect(GridPosition bottomLeft, uint width, uint height)
    {
        highlightManager.AddBlockHighlightXyRect(bottomLeft, width, height);
    }

    /// <summary>
    ///     Synchronously adds block highlights to a square centered on the given position.
    /// </summary>
    /// <param name="center">Center position.</param>
    /// <param name="width">Width of the full square in blocks.</param>
    public void AddBlockHighlightSquare(GridPosition center, uint width)
    {
        highlightManager.AddBlockHighlightSquare(center, width);
    }

    /// <summary>
    ///     Synchronously sets the block highlight state.
    /// </summary>
    /// <param name="state"></param>
    public void SetBlockHighlightState(Highlight state)
    {
        highlightManager.BlockHighlightState = state;
    }
}