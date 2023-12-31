﻿/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Castle.Core.Logging;
using Sovereign.EngineUtil.Collections;
using Sovereign.EngineUtil.Monads;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Base class of component collections.
/// </summary>
/// Component values are updated asynchronously by enqueuing add, remove, and
/// modify operations. These operations are executed on the main thread at the
/// transition between event ticks in the following order:
/// <list type="number">
///     <item>Add</item>
///     <item>Modify</item>
///     <item>Remove</item>
/// </list>
/// Operations are enqueued with respect to the current state of the component;
/// for example, RemoveComponent() cannot be called to remove a component that
/// is currently enqueued for addition.
/// <typeparam name="T">Component value type.</typeparam>
public class BaseComponentCollection<T> : IComponentUpdater, IComponentEventSource<T>, IComponentRemover
    where T : notnull
{
    /// <summary>
    ///     Internal operation buffer size.
    /// </summary>
    private const int OperationBufferSize = 1024;

    /// <summary>
    ///     Underlying component array.
    /// </summary>
    private readonly List<T> components;

    /// <summary>
    ///     Map from component buffer index to associated entity ID.
    /// </summary>
    private readonly ConcurrentDictionary<int, ulong> componentToEntityMap = new();

    /// <summary>
    ///     Map from entity ID to associated component.
    /// </summary>
    private readonly ConcurrentDictionary<ulong, int> entityToComponentMap = new();

    /// <summary>
    ///     Buffer indices ready for reuse.
    /// </summary>
    private readonly Queue<int> indexQueue = new();

    /// <summary>
    ///     Operators associated with this component.
    /// </summary>
    private readonly Dictionary<ComponentOperation, Func<T, T, T>> operators;

    /// <summary>
    ///     Entity IDs of pending adds.
    /// </summary>
    private readonly HashSet<ulong> pendingAddEntityIds = new();

    /// <summary>
    ///     Add events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingAddEvents = new();

    /// <summary>
    ///     Pending component additions.
    /// </summary>
    private readonly StructBuffer<PendingAdd> pendingAdds = new(OperationBufferSize);

    /// <summary>
    ///     Pending component modifications binned by operation.
    /// </summary>
    private readonly Dictionary<ComponentOperation, StructBuffer<PendingModify>>
        pendingModifications = new();

    /// <summary>
    ///     Modify events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingModifyEvents = new();

    /// <summary>
    ///     Remove events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingRemoveEvents = new();

    /// <summary>
    ///     Pending component removals.
    /// </summary>
    private readonly StructBuffer<PendingRemove> pendingRemoves = new(OperationBufferSize);

    /// <summary>
    ///     Unload events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingUnloadEvents = new();

    /// <summary>
    ///     Pending component unloads.
    /// </summary>
    private readonly StructBuffer<PendingUnload> pendingUnloads = new(OperationBufferSize);

    /// <summary>
    ///     Creates a base component collection.
    /// </summary>
    /// <param name="componentManager">Component manager.</param>
    /// <param name="initialSize">Initial size of the component buffer.</param>
    /// <param name="operators">Dict of component operators for use in updates.</param>
    /// <param name="componentType">Component type.</param>
    protected BaseComponentCollection(ComponentManager componentManager, int initialSize,
        Dictionary<ComponentOperation, Func<T, T, T>> operators,
        ComponentType componentType)
    {
        this.operators = operators;
        components = new List<T>(initialSize);

        ComponentType = componentType;

        /* Key up the allowed operators. */
        foreach (var operation in operators.Keys)
            pendingModifications[operation] = new StructBuffer<PendingModify>(OperationBufferSize);

        /* Register with the component manager. */
        componentManager.RegisterComponentUpdater(this);
        componentManager.RegisterComponentRemover(this);
    }

    public ILogger Log { private get; set; } = NullLogger.Instance;

    /// <summary>
    ///     Component type.
    /// </summary>
    public ComponentType ComponentType { get; private set; }

    /// <summary>
    ///     Provides access to the components indexed by the associated
    ///     entity ID.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Component associated with the given entity.</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown by the getter if no component is associated with the entity ID.
    /// </exception>
    public T this[ulong entityId] => components[entityToComponentMap[entityId]];

    public event Action? OnStartUpdates;

    public event ComponentEventDelegates<T>.ComponentEventHandler? OnComponentAdded;

    public event ComponentEventDelegates<T>.ComponentRemovedEventHandler? OnComponentRemoved;

    public event ComponentEventDelegates<T>.ComponentEventHandler? OnComponentModified;

    public event ComponentEventDelegates<T>.ComponentUnloadedEventHandler? OnComponentUnloaded;

    public event Action? OnEndUpdates;

    public void RemoveComponent(ulong entityId)
    {
        /* Ensure that a component is associated. */
        if (!HasComponentForEntity(entityId)) return;

        /* Enqueue the removal. */
        var pendingRemove = new PendingRemove
        {
            EntityId = entityId
        };
        pendingRemoves.Add(ref pendingRemove);
    }

    public void UnloadComponent(ulong entityId)
    {
        /* Ensure that a component is associated. */
        if (!HasComponentForEntity(entityId)) return;

        /* Enqueue the unload. */
        var pendingUnload = new PendingUnload
        {
            EntityId = entityId
        };
        pendingUnloads.Add(ref pendingUnload);
    }

    /// <summary>
    ///     Applies pending component updates.
    /// </summary>
    public void ApplyComponentUpdates()
    {
        /* Apply all pending operations. */
        ApplyAdds();
        ApplyModifications();
        ApplyRemoves();
        ApplyUnloads();

        /* Dispatch events as needed. */
        FireComponentEvents();
    }

    /// <summary>
    ///     Enqueues the creation of a new component associated with the given entity.
    ///     If a component of this type is already associated with the entity, the
    ///     existing component will be updated to the given initial value.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="initialValue">Initial value of the component.</param>
    /// <exception cref="NotSupportedException">
    ///     Thrown if a component is already associated with the entity, and the
    ///     Set operation is not supported.
    /// </exception>
    public void AddComponent(ulong entityId, T initialValue)
    {
        if (HasComponentForEntity(entityId))
        {
            /* Component already exists - enqueue an update. */
            ModifyComponent(entityId, ComponentOperation.Set, initialValue);

            Log.WarnFormat("Attempted duplicate component addition for entity {1}.", entityId);
        }
        else
        {
            /* Component does not exist - enqueue an add. */
            var pendingAdd = new PendingAdd
            {
                EntityId = entityId,
                InitialValue = initialValue
            };
            pendingAdds.Add(ref pendingAdd);
            pendingAddEntityIds.Add(entityId);
        }
    }

    /// <summary>
    ///     Enqueues the modification of the component associated with the given entity.
    /// </summary>
    /// Note that the component must already exist and be associated with the given
    /// entity; it is not sufficient to first make the corresponding call to
    /// AddComponent. If the component is not associated, this method has no effect.
    /// <param name="entityId">Entity ID.</param>
    /// <param name="operation">Operation to perform on the component.</param>
    /// <param name="adjustment">Adjustment value.</param>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the requested operation is not supported for this component.
    /// </exception>
    public void ModifyComponent(ulong entityId, ComponentOperation operation, T adjustment)
    {
        /* Ensure that the entity has an associated component. */
        if (!HasComponentForEntity(entityId)) return;
        var componentIndex = entityToComponentMap[entityId];

        /* Ensure that the operation is supported by this component. */
        if (!operators.ContainsKey(operation))
            throw new NotSupportedException();

        /* Enqueue a modification. */
        var pendingModify = new PendingModify
        {
            ComponentIndex = componentIndex,
            ComponentOperation = operation,
            Adjustment = adjustment
        };
        pendingModifications[operation].Add(ref pendingModify);
    }

    /// <summary>
    ///     Determines whether a component is associated with the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if a component is associated, false otherwise.</returns>
    public bool HasComponentForEntity(ulong entityId)
    {
        return entityToComponentMap.ContainsKey(entityId);
    }

    /// <summary>
    ///     Determines whether a component will be associated with the given entity
    ///     following the next commit of pending adds.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>true if there is a pending add, false otherwise.</returns>
    public bool HasPendingComponentForEntity(ulong entityId)
    {
        return pendingAddEntityIds.Contains(entityId);
    }

    /// <summary>
    ///     Gets the component associated with the given entity as a Maybe.
    ///     Note that this method has increased overhead due to the need to
    ///     instantiate
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Component associated with the given entity.</returns>
    public Maybe<T> GetComponentForEntity(ulong entityId)
    {
        var maybe = new Maybe<T>();
        if (entityToComponentMap.TryGetValue(entityId, out var componentId))
            maybe.Value = components[componentId];
        return maybe;
    }

    /// <summary>
    ///     Creates a dictionary mapping entity IDs to all known components.
    /// </summary>
    /// This method should be used sparingly - it requires allocation and creation of
    /// an entire dictionary. It is intended to be used in the initial creation of
    /// a ComponentIndexer. Consider using GetComponentForEntity() in combination
    /// with an appropriate ComponentIndexer instead.
    /// <returns>Dictionary mapping entity IDs to all known components.</returns>
    public IDictionary<ulong, T> GetAllComponents()
    {
        var dict = new Dictionary<ulong, T>();
        foreach (var entityId in entityToComponentMap.Keys) dict[entityId] = components[entityToComponentMap[entityId]];
        return dict;
    }

    /// <summary>
    ///     Fires all pending component events.
    /// </summary>
    private void FireComponentEvents()
    {
        /* Announce that events are being fired. */
        OnStartUpdates?.Invoke();

        /* Fire events. */
        FireAddEvents();
        FireModificationEvents();
        FireRemoveEvents();
        FireUnloadEvents();

        /* Announce that events are done being fired. */
        OnEndUpdates?.Invoke();
    }

    /// <summary>
    ///     Applies all pending component additions.
    /// </summary>
    private void ApplyAdds()
    {
        /* Iterate over new components. */
        foreach (var pendingAdd in pendingAdds)
        {
            ApplyAddComponent(pendingAdd.EntityId, pendingAdd.InitialValue);
            pendingAddEvents.Add(pendingAdd.EntityId);
        }

        /* Reset the buffer. */
        pendingAddEntityIds.Clear();
        pendingAdds.Clear();
    }

    /// <summary>
    ///     Applies all pending component modifications.
    /// </summary>
    private void ApplyModifications()
    {
        /* Iterate over operations. */
        foreach (var operation in pendingModifications.Keys)
        {
            /* Transform and update all components. */
            var op = operators[operation];
            var queue = pendingModifications[operation];
            foreach (var pendingModify in queue)
            {
                var transformed = op(components[pendingModify.ComponentIndex],
                    pendingModify.Adjustment);
                components[pendingModify.ComponentIndex] = transformed;
            }

            /* Reset the buffer. */
            queue.Clear();
        }
    }

    /// <summary>
    ///     Applies all pending component removals.
    /// </summary>
    private void ApplyRemoves()
    {
        /* Apply operations. */
        foreach (var pendingRemove in pendingRemoves) ApplyRemoveOrUnloadComponent(pendingRemove.EntityId);

        /* Clear the buffer. */
        pendingRemoves.Clear();
    }

    /// <summary>
    ///     Applies all pending component unloads.
    /// </summary>
    private void ApplyUnloads()
    {
        /* Apply operations. */
        foreach (var pendingUnload in pendingUnloads) ApplyRemoveOrUnloadComponent(pendingUnload.EntityId);

        /* Clear the buffer. */
        pendingUnloads.Clear();
    }

    /// <summary>
    ///     Fires events for component additions.
    /// </summary>
    private void FireAddEvents()
    {
        /* Iterate over entities that have been added. */
        if (OnComponentAdded != null)
            foreach (var entityId in pendingAddEvents)
            {
                /* Notify all listeners. */
                var value = this[entityId];
                OnComponentAdded?.Invoke(entityId, value);
            }

        /* Reset the pending events. */
        pendingAddEvents.Clear();
    }

    /// <summary>
    ///     Fires events for component modifications.
    /// </summary>
    private void FireModificationEvents()
    {
        /* Iterate over entities that have been modified. */
        if (OnComponentModified != null)
            foreach (var entityId in pendingModifyEvents)
            {
                /* Notify all listeners. */
                var value = this[entityId];
                OnComponentModified.Invoke(entityId, value);
            }

        /* Reset the pending events. */
        pendingModifyEvents.Clear();
    }

    /// <summary>
    ///     Fires events for component removals.
    /// </summary>
    private void FireRemoveEvents()
    {
        /* Iterate over entities that have been removed. */
        if (OnComponentRemoved != null)
            foreach (var entityId in pendingRemoveEvents)
                /* Notify all listeners. */
                OnComponentRemoved.Invoke(entityId);

        /* Reset the pending events. */
        pendingRemoveEvents.Clear();
    }

    /// <summary>
    ///     Fires events for component unloads.
    /// </summary>
    private void FireUnloadEvents()
    {
        /* Iterate over entities that have been unloaded. */
        if (OnComponentUnloaded != null)
            foreach (var entityId in pendingUnloadEvents)
                /* Notify all listeners. */
                OnComponentUnloaded.Invoke(entityId);

        /* Reset the pending events. */
        pendingUnloadEvents.Clear();
    }

    /// <summary>
    ///     Immediately adds a component for the given entity.
    ///     This method should only be used from the main thread.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="componentValue">Initial value of the new component.</param>
    private void ApplyAddComponent(ulong entityId, T componentValue)
    {
        /* Insert the component into the buffer. */
        var index = AddComponentToBuffer(componentValue);

        /* Create the appropriate indices into the buffer. */
        RegisterIndices(index, entityId);
    }

    /// <summary>
    ///     Immediately removes or unloads the component associated with the given entity.
    ///     If no component is associated, this method has no effect.
    ///     This method should only be used from the main thread.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void ApplyRemoveOrUnloadComponent(ulong entityId)
    {
        /* Ensure that a component is associated to the entity. */
        if (!entityToComponentMap.ContainsKey(entityId)) return;

        /* Remove the component, but leave it allocated for later reuse. */
        var index = entityToComponentMap[entityId];
        entityToComponentMap.Remove(entityId, out _);
        indexQueue.Enqueue(index);
    }

    /// <summary>
    ///     Adds a component to the buffer.
    /// </summary>
    /// <param name="componentValue">Component value.</param>
    /// <returns>
    ///     Index into the component buffer at which the component
    ///     is located.
    /// </returns>
    private int AddComponentToBuffer(T componentValue)
    {
        int index;
        if (indexQueue.Count > 0)
        {
            /* Reuse a previously deleted component. */
            index = indexQueue.Dequeue();
            components[index] = componentValue;
        }
        else
        {
            /* Append to the next position. */
            index = components.Count;
            components.Add(componentValue);
        }

        return index;
    }

    /// <summary>
    ///     Registers the appropriate lookup indices for the given
    ///     component.
    /// </summary>
    /// <param name="bufferIndex">Buffer index of the component.</param>
    /// <param name="entityId">ID of the associated entity.</param>
    private void RegisterIndices(int bufferIndex, ulong entityId)
    {
        entityToComponentMap[entityId] = bufferIndex;
        componentToEntityMap[bufferIndex] = entityId;
    }

    /// <summary>
    ///     Describes a pending component modification.
    /// </summary>
    [DebuggerDisplay("{ComponentIndex} => {ComponentOperation} {Adjustment}")]
    private struct PendingModify
    {
        /// <summary>
        ///     Component index for update.
        /// </summary>
        public int ComponentIndex;

        /// <summary>
        ///     Operation to perform on the component.
        /// </summary>
        public ComponentOperation ComponentOperation;

        /// <summary>
        ///     Adjustment to the component value.
        /// </summary>
        public T Adjustment;
    }

    /// <summary>
    ///     Describes a pending component creation.
    /// </summary>
    [DebuggerDisplay("{EntityId} => {InitialValue}")]
    private struct PendingAdd
    {
        /// <summary>
        ///     ID of the associated entity.
        /// </summary>
        public ulong EntityId;

        /// <summary>
        ///     Initial value of the new component.
        /// </summary>
        public T InitialValue;
    }

    /// <summary>
    ///     Describes a pending component removal.
    /// </summary>
    [DebuggerDisplay("{EntityId}")]
    private struct PendingRemove
    {
        /// <summary>
        ///     ID of the associated entity.
        /// </summary>
        public ulong EntityId;
    }

    [DebuggerDisplay("{EntityId}")]
    private struct PendingUnload
    {
        /// <summary>
        ///     ID of the associated entity.
        /// </summary>
        public ulong EntityId;
    }
}