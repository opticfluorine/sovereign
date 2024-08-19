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

using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

public class TemplateEntityInternalController
{
    private readonly EntityDefinitionGenerator definitionGenerator;

    public TemplateEntityInternalController(EntityDefinitionGenerator definitionGenerator)
    {
        this.definitionGenerator = definitionGenerator;
    }

    /// <summary>
    ///     Synchronizes a template entity to all connected clients.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="templateEntityId">Template entity ID.</param>
    public void SyncTemplateEntity(IEventSender eventSender, ulong templateEntityId)
    {
        var details = new TemplateEntityDefinitionEventDetails
        {
            Definition = definitionGenerator.GenerateDefinition(templateEntityId)
        };
        eventSender.SendEvent(new Event(EventId.Client_EntitySynchronization_SyncTemplate, details));
    }
}