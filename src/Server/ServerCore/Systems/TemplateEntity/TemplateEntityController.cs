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

using System.Collections.Generic;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     Controller for TemplateEntity system.
/// </summary>
public sealed class TemplateEntityController
{
    /// <summary>
    ///     Creates or updates a template entity including its key-value pairs.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="definition">Entity definition.</param>
    /// <param name="keyValuePairs">Entity key-value pairs.</param>
    public void UpdateTemplateEntity(IEventSender eventSender, EntityDefinition definition,
        Dictionary<string, string> keyValuePairs)
    {
        var details = new KeyedEntityDefinitionEventDetails
        {
            EntityDefinition = definition,
            EntityKeyValuePairs = keyValuePairs
        };
        var ev = new Event(EventId.Server_TemplateEntity_UpdateKeyed, details);
        eventSender.SendEvent(ev);
    }
}