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

using Microsoft.Extensions.Logging;
using Sovereign.EngineCore.Entities;

namespace Sovereign.ClientCore.Entities;

/// <summary>
///     Responsible for loading template entity data in the client.
/// </summary>
public class TemplateEntityDataLoader
{
    private readonly EntityDefinitionProcessor definitionProcessor;
    private readonly ILogger<TemplateEntityDataLoader> logger;

    public TemplateEntityDataLoader(EntityDefinitionProcessor definitionProcessor,
        ILogger<TemplateEntityDataLoader> logger)
    {
        this.definitionProcessor = definitionProcessor;
        this.logger = logger;
    }

    /// <summary>
    ///     Loads a set of template entity data into the entity and component tables.
    /// </summary>
    /// <param name="data">Data.</param>
    public void Load(TemplateEntityData data)
    {
        foreach (var def in data.TemplateEntityDefinitions)
        {
            definitionProcessor.ProcessDefinition(def);
        }

        logger.LogInformation("Loaded {Count} template entities from server.", data.TemplateEntityDefinitions.Count);
    }
}