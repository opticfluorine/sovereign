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

using System.Numerics;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Internal controller used by the ClientWorldEdit system.
/// </summary>
public class ClientWorldEditInternalController
{
    /// <summary>
    ///     Requests the server to set a block at the given position.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="position">Block position.</param>
    /// <param name="blockTemplateId">Block template entity ID.</param>
    public void SetBlock(IEventSender eventSender, GridPosition position, ulong blockTemplateId)
    {
        var details = new BlockAddEventDetails
        {
            BlockRecord = new BlockRecord
            {
                Position = position,
                TemplateEntityId = blockTemplateId
            }
        };
        var ev = new Event(EventId.Server_WorldEdit_SetBlock, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests the server to remove a block at the given position.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="position">Block position.</param>
    public void RemoveBlock(IEventSender eventSender, GridPosition position)
    {
        var details = new GridPositionEventDetails { GridPosition = position };
        var ev = new Event(EventId.Server_WorldEdit_RemoveBlock, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests the server to add a non-block entity at the given position with the specified template.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="position">Position.</param>
    /// <param name="templateId">Non-block template entity ID.</param>
    public void AddNonBlock(IEventSender eventSender, Vector3 position, ulong templateId)
    {
        var details = new NonBlockAddEventDetails
        {
            Position = position,
            TemplateId = templateId
        };
        var ev = new Event(EventId.Server_WorldEdit_AddNonBlock, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Requests the server to remove a non-block entity by entity ID.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="npcEntityId">Non-block entity ID.</param>
    public void RemoveNonBlock(IEventSender eventSender, ulong npcEntityId)
    {
        var details = new NonBlockRemoveEventDetails
        {
            EntityId = npcEntityId
        };
        var ev = new Event(EventId.Server_WorldEdit_RemoveNonBlock, details);
        eventSender.SendEvent(ev);
    }
}