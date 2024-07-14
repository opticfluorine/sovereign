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

using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Public asynchronous API for interacting with the ClientWorldEdit system.
/// </summary>
public class ClientWorldEditController
{
    /// <summary>
    ///     Sets the selected material for the world editor.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="materialId">Material ID.</param>
    /// <param name="materialModifier">Material modifier.</param>
    public void SetSelectedMaterial(IEventSender eventSender, int materialId, int materialModifier)
    {
        var details = new MaterialPairEventDetails
        {
            MaterialPair = new MaterialPair(materialId, materialModifier)
        };
        var ev = new Event(EventId.Client_WorldEdit_SetMaterial, details);
        eventSender.SendEvent(ev);
    }

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
}