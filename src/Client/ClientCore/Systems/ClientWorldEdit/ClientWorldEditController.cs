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

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Public asynchronous API for interacting with the ClientWorldEdit system.
/// </summary>
public class ClientWorldEditController
{
    /// <summary>
    ///     Sets the selected Z-offset for the world editor.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="zOffset">Z-offset.</param>
    public void SetZOffset(IEventSender eventSender, int zOffset)
    {
        var details = new GenericEventDetails<int>
        {
            Value = zOffset
        };
        var ev = new Event(EventId.Client_WorldEdit_SetZOffset, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sets the pen width for the world editor.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="penWidth">Pen width.</param>
    public void SetPenWidth(IEventSender eventSender, int penWidth)
    {
        var details = new GenericEventDetails<int>
        {
            Value = penWidth
        };
        var ev = new Event(EventId.Client_WorldEdit_SetPenWidth, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sets the tool for the world editor.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="worldEditTool">Tool.</param>
    public void SetTool(IEventSender eventSender, WorldEditTool worldEditTool)
    {
        var details = new GenericEventDetails<WorldEditTool>
        {
            Value = worldEditTool
        };
        var ev = new Event(EventId.Client_WorldEdit_SetTool, details);
        eventSender.SendEvent(ev);
    }

    /// <summary>
    ///     Sets the snap-to-grid value for the world editor.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="snapToGrid">Snap to grid value.</param>
    public void SetSnapToGrid(IEventSender eventSender, bool snapToGrid)
    {
        var details = new BooleanEventDetails
        {
            Value = snapToGrid
        };
        var ev = new Event(EventId.Client_WorldEdit_SetSnapToGrid, details);
        eventSender.SendEvent(ev);
    }
}