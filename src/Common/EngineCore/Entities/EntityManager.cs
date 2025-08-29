/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Sovereign.EngineCore.Components;
using Sovereign.EngineUtil.Threading;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Top-level manager of the entity infrastructure.
/// </summary>
/// The methods exposed by this class are thread-safe.
public class EntityManager
{
    private readonly ComponentManager componentManager;
    private readonly EntityNotifier entityNotifier;
    private readonly EntityTable entityTable;

    public EntityManager(ComponentManager componentManager,
        EntityNotifier entityNotifier,
        EntityTable entityTable)
    {
        this.componentManager = componentManager;
        this.entityNotifier = entityNotifier;
        this.entityTable = entityTable;
    }

    /// <summary>
    ///     Incremental guard used to synchronize component updates.
    /// </summary>
    public IncrementalGuard UpdateGuard { get; } = new();

    /// <summary>
    ///     Applies all entity and component updates.
    /// </summary>
    public void ApplyUpdates()
    {
        using var strongLock = UpdateGuard.AcquireStrongLock();

        OnUpdatesStarted?.Invoke();
        componentManager.UpdateAllComponents();
        entityTable.UpdateAllEntities();
        OnUpdatesComplete?.Invoke();
    }

    /// <summary>
    ///     Removes the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID to be removed.</param>
    public void RemoveEntity(ulong entityId)
    {
        componentManager.RemoveAllComponentsForEntity(entityId);
        entityNotifier.EnqueueRemove(entityId);
        entityTable.Remove(entityId, false);
    }

    /// <summary>
    ///     Unloads the given entity, but does not formally delete it.
    /// </summary>
    /// <param name="entityId">Entity ID to be unloaded.</param>
    public void UnloadEntity(ulong entityId)
    {
        componentManager.UnloadAllComponentsForEntity(entityId);
        entityNotifier.EnqueueUnload(entityId);
        entityTable.Remove(entityId, true);
    }

    /// <summary>
    ///     Unloads all entities.
    /// </summary>
    /// <param name="excludeTemplates">If true, does not unload template entities.</param>
    public void UnloadAll(bool excludeTemplates)
    {
        foreach (var entityId in entityTable.EntityIds)
        {
            if (excludeTemplates &&
                entityId is >= EntityConstants.FirstTemplateEntityId and <= EntityConstants.LastTemplateEntityId)
                continue;

            UnloadEntity(entityId);
        }
    }

    /// <summary>
    ///     Event triggered when a round of entity/component updates are started.
    /// </summary>
    public event Action? OnUpdatesStarted;

    /// <summary>
    ///     Event triggered when a round of entity/component updates are complete.
    /// </summary>
    public event Action? OnUpdatesComplete;
}