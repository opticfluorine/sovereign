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
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.State.Trackers;

/// <summary>
///     Custom state tracker for entity template data.
/// </summary>
public class TemplateStateTracker
{
    private readonly EntityMapper entityMapper;
    private readonly EntityTable entityTable;
    private readonly StateManager stateManager;

    public TemplateStateTracker(EntityTable entityTable, StateManager stateManager, EntityMapper entityMapper)
    {
        this.entityTable = entityTable;
        this.stateManager = stateManager;
        this.entityMapper = entityMapper;
        entityTable.OnTemplateSet += OnTemplateSet;
    }

    /// <summary>
    ///     Called when a template is added, modified, or removed for an entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="templateEntityId">Template entity ID, or 0 for no template.</param>
    /// <param name="isLoad">Load flag.</param>
    private void OnTemplateSet(ulong entityId, ulong templateEntityId, bool isLoad)
    {
        if (isLoad || !entityTable.IsPersisted(entityId)) return;
        var persistedId = GetPersistedId(entityId);
        var update = new StateUpdate<ulong>
            { EntityId = persistedId, StateUpdateType = StateUpdateType.Modify, Value = templateEntityId };
        stateManager.FrontBuffer.UpdateTemplate(ref update);
    }

    /// <summary>
    ///     Gets the persisted ID for the given entity ID, queueing an entity
    ///     creation in the database if needed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Persisted entity ID.</returns>
    private ulong GetPersistedId(ulong entityId)
    {
        var persistedId = entityMapper.GetPersistedId(entityId,
            out var needToCreate);
        if (needToCreate) stateManager.FrontBuffer.AddEntity(persistedId);
        return persistedId;
    }
}