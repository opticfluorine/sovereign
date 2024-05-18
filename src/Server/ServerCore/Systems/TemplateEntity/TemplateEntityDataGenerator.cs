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

using System.Threading.Tasks;
using MessagePack;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Network;
using Sovereign.ServerCore.Entities;

namespace Sovereign.ServerCore.Systems.TemplateEntity;

/// <summary>
///     Generates and maintains the up-to-date TemplateEntityData payload for TemplateEntitiesRestService.
/// </summary>
public class TemplateEntityDataGenerator
{
    private readonly EntityDefinitionGenerator definitionGenerator;
    private readonly EntityTable entityTable;

    /// <summary>
    ///     Cache invalidation flag.
    /// </summary>
    private bool invalidated;

    /// <summary>
    ///     Asynchronous task yielding the latest data in compressed serialized form.
    /// </summary>
    private Task<byte[]>? latestTemplateEntityData;

    public TemplateEntityDataGenerator(EntityTable entityTable, EntityDefinitionGenerator definitionGenerator)
    {
        this.entityTable = entityTable;
        this.definitionGenerator = definitionGenerator;
    }

    /// <summary>
    ///     Gets an asynchronous task that yields the latest TemplateEntityData object
    ///     in compressed serialized form.
    /// </summary>
    /// <returns>Latest TemplateEntityData object in compressed serialized form.</returns>
    public Task<byte[]> GetLatestTemplateEntityData()
    {
        if (invalidated || latestTemplateEntityData == null)
        {
            latestTemplateEntityData = Task.Run(CreateTemplateEntityData);
            invalidated = false;
        }

        return latestTemplateEntityData;
    }

    /// <summary>
    ///     Called when a change to entities or components invalidates the current template entity data.
    /// </summary>
    public void OnTemplatesChanged()
    {
        invalidated = true;
    }

    /// <summary>
    ///     Creates and serializes the latest TemplateEntityData.
    /// </summary>
    /// <returns>Compressed and serialized TemplateEntityData object.</returns>
    private byte[] CreateTemplateEntityData()
    {
        var data = BuildDataList();
        return MessagePackSerializer.Serialize(data, MessageConfig.CompressedUntrustedMessagePackOptions);
    }

    /// <summary>
    ///     Generates the latest TemplateEntityData object.
    /// </summary>
    /// <returns>TemplateEntityData object.</returns>
    private TemplateEntityData BuildDataList()
    {
        var data = new TemplateEntityData();
        for (var entityId = EntityConstants.FirstTemplateEntityId;
             entityId < entityTable.NextTemplateEntityId;
             ++entityId)
        {
            if (!entityTable.Exists(entityId)) continue;
            data.TemplateEntityDefinitions.Add(definitionGenerator.GenerateDefinition(entityId));
        }

        return data;
    }
}