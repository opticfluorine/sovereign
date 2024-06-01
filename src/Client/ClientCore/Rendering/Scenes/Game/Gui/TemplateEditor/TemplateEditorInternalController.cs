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

using System.Collections.Generic;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Events.Details;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.Gui.TemplateEditor;

/// <summary>
///     Internal controller for the template editor GUI.
/// </summary>
public class TemplateEditorInternalController
{
    /// <summary>
    ///     Remotely updates a template entity on the server.
    /// </summary>
    /// <param name="eventSender">Event sender.</param>
    /// <param name="definition">Entity definition.</param>
    public void UpdateTemplateEntity(IEventSender eventSender, EntityDefinition definition)
    {
        var details = new EntityDefinitionEventDetails
        {
            EntityDefinitions = new List<EntityDefinition> { definition }
        };
        var ev = new Event(EventId.Server_TemplateEntity_Update, details);
        eventSender.SendEvent(ev);
    }
}