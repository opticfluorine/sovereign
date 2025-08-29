/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Entities;

namespace Sovereign.Persistence.State;

/// <summary>
///     Base class for state trackers.
/// </summary>
/// <typeparam name="T">Component type.</typeparam>
public abstract class BaseStateTracker<T> : IDisposable
    where T : notnull
{
    private readonly BaseComponentCollection<T> components;
    private readonly T defaultElement;
    private readonly EntityTable entityTable;
    private readonly ExistingEntitySet existingEntitySet;
    protected readonly StateManager stateManager;

    /// <summary>
    ///     Creates the base state tracker.
    /// </summary>
    /// <param name="components">Component collection.</param>
    /// <param name="defaultElement">Default value for elements.</param>
    /// <param name="stateManager">State manager.</param>
    /// <param name="entityTable">Entity table.</param>
    /// <param name="existingEntitySet">Existing entity set.</param>
    public BaseStateTracker(BaseComponentCollection<T> components,
        T defaultElement, ExistingEntitySet existingEntitySet, StateManager stateManager, EntityTable entityTable)
    {
        this.components = components;
        this.defaultElement = defaultElement;
        this.stateManager = stateManager;
        this.entityTable = entityTable;
        this.existingEntitySet = existingEntitySet;

        RegisterCallbacks();
    }

    public void Dispose()
    {
        DeregisterCallbacks();
    }

    /// <summary>
    ///     Called when a state update is available.
    /// </summary>
    /// <param name="update">State update.</param>
    protected abstract void OnStateUpdate(ref StateUpdate<T> update);

    /// <summary>
    ///     Called when a component is added.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">Component value.</param>
    /// <param name="isLoad">If true, this is a loaded component that should not be re-added to the database.</param>
    private void OnComponentAdded(ulong entityId, T componentValue, bool isLoad)
    {
        if (!entityTable.IsPersisted(entityId)) return;
        CreateIfNeeded(entityId, isLoad);
        if (isLoad) return;

        var update = new StateUpdate<T>
        {
            EntityId = entityId,
            StateUpdateType = StateUpdateType.Add,
            Value = componentValue
        };
        OnStateUpdate(ref update);
    }

    /// <summary>
    ///     Called when a component is modified.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">Component value.</param>
    private void OnComponentModified(ulong entityId, T componentValue)
    {
        if (!entityTable.IsPersisted(entityId)) return;
        CreateIfNeeded(entityId, false);

        var update = new StateUpdate<T>
        {
            EntityId = entityId,
            StateUpdateType = StateUpdateType.Modify,
            Value = componentValue
        };
        OnStateUpdate(ref update);
    }

    /// <summary>
    ///     Called when a component is removed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">If true, this is just an unload from memory - do not modify the database.</param>
    private void OnComponentRemoved(ulong entityId, bool isUnload)
    {
        if (!entityTable.IsPersisted(entityId)) return;
        CreateIfNeeded(entityId, isUnload);
        if (isUnload) return;

        var update = new StateUpdate<T>
        {
            EntityId = entityId,
            StateUpdateType = StateUpdateType.Remove,
            Value = defaultElement
        };
        OnStateUpdate(ref update);
    }

    /// <summary>
    ///     Registers the component callbacks.
    /// </summary>
    private void RegisterCallbacks()
    {
        components.OnComponentAdded += OnComponentAdded;
        components.OnComponentModified += OnComponentModified;
        components.OnComponentRemoved += OnComponentRemoved;
    }

    /// <summary>
    ///     Deregisters the component callbacks.
    /// </summary>
    private void DeregisterCallbacks()
    {
        components.OnComponentRemoved -= OnComponentRemoved;
        components.OnComponentModified -= OnComponentModified;
        components.OnComponentAdded -= OnComponentAdded;
    }

    /// <summary>
    ///     Marks the entity for creation if needed.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isLoad">Load flag.</param>
    private void CreateIfNeeded(ulong entityId, bool isLoad)
    {
        if (existingEntitySet.MarkAsExists(entityId) && !isLoad) stateManager.FrontBuffer.AddEntity(entityId);
    }
}