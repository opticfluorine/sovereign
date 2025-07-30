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
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.EngineCore.Systems.Interaction;

/// <summary>
///     Controller class for the Interaction system.
/// </summary>
public class InteractionController
{
    /// <summary>
    ///     Requests an interaction with an entity without using a tool.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    public void Interact(IEventSender eventSender, ulong sourceEntityId, ulong targetEntityId)
    {
        Interact(eventSender, sourceEntityId, targetEntityId, sourceEntityId);
    }

    /// <summary>
    ///     Requests an inteaction with an entity using a tool.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="sourceEntityId">Source entity ID.</param>
    /// <param name="targetEntityId">Target entity ID.</param>
    /// <param name="toolEntityId">Tool entity ID.</param>
    public void Interact(IEventSender eventSender, ulong sourceEntityId, ulong targetEntityId, ulong toolEntityId)
    {
        var details = new InteractEventDetails
        {
            SourceEntityId = sourceEntityId,
            TargetEntityId = targetEntityId,
            ToolEntityId = toolEntityId
        };
        var ev = new Event(EventId.Core_Interaction_Interact, details);
        eventSender.SendEvent(ev);
    }
}