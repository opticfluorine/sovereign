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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sovereign.EngineCore.Entities;
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

    private readonly EntityTable entityTable;

    /// <summary>
    ///     Map from entity ID to associated component.
    /// </summary>
    private readonly Dictionary<ulong, int> entityToComponentMap = new();

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
    private readonly ConcurrentBag<ulong> pendingAddEntityIds = new();

    /// <summary>
    ///     Add events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingAddEvents = new();

    /// <summary>
    ///     Pending component additions.
    /// </summary>
    private readonly StructBuffer<PendingAdd> pendingAdds = new(OperationBufferSize);

    /// <summary>
    ///     Load events that are pending invocation.
    /// </summary>
    private readonly HashSet<ulong> pendingLoadEvents = new();

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
    ///     Components ready to be reclaimed on the next tick after a remove/unload.
    /// </summary>
    private readonly HashSet<ulong> pendingReclaims = new();

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
    ///     Increment to increase backing array size as necessary.
    /// </summary>
    private readonly int resizeIncrement;

    /// <summary>
    ///     Underlying component array.
    /// </summary>
    private T[] components;

    /// <summary>
    ///     Map from component buffer index to associated entity ID.
    /// </summary>
    private ulong[] componentToEntityMap;

    /// <summary>
    ///     Indices of components modified by a direct access.
    /// </summary>
    private int[] directAccessModifiedIndices;

    /// <summary>
    ///     When true, allows direct iteration of the components by systems.
    /// </summary>
    private bool enableDirectAccess;

    /// <summary>
    ///     Flag indicating the component collection currently has adds.
    /// </summary>
    private bool hasAdds;

    /// <summary>
    ///     Flag indicating the component collection currently has modifications.
    /// </summary>
    private bool hasModifications;

    /// <summary>
    ///     Flag indicating the component collection currently has reclaims.
    /// </summary>
    private bool hasReclaims;

    /// <summary>
    ///     Flag indicating the component collection currently has removes.
    /// </summary>
    private bool hasRemoves;

    /// <summary>
    ///     Next unused index, not counting anything in the queue.
    /// </summary>
    private int nextIndex;

    /// <summary>
    ///     Creates a base component collection.
    /// </summary>
    /// <param name="entityTable">Entity table.</param>
    /// <param name="componentManager">Component manager.</param>
    /// <param name="initialSize">Initial size of the component buffer.</param>
    /// <param name="operators">Dict of component operators for use in updates.</param>
    /// <param name="componentType">Component type.</param>
    protected BaseComponentCollection(EntityTable entityTable, ComponentManager componentManager, int initialSize,
        Dictionary<ComponentOperation, Func<T, T, T>> operators,
        ComponentType componentType)
    {
        this.entityTable = entityTable;
        this.operators = operators;
        components = new T[initialSize];
        directAccessModifiedIndices = new int[initialSize];
        componentToEntityMap = new ulong[initialSize];
        resizeIncrement = initialSize;

        ComponentType = componentType;

        /* Key up the allowed operators. */
        foreach (var operation in operators.Keys)
            pendingModifications[operation] = new StructBuffer<PendingModify>(OperationBufferSize);

        /* Register with the component manager. */
        componentManager.RegisterComponentUpdater(this);
        componentManager.RegisterComponentRemover(this);
    }

    /// <summary>
    ///     Provides direct access to the underlying component list. Only allowed during the direct access
    ///     phase of component update processing.
    /// </summary>
    /// <remarks>
    ///     To perform direct iteration of components, subscribe to the OnBeginDirectAccess event and accept the
    ///     update index list. If you modify any components, append to this list the index at which the modification
    ///     was made. Indices in this list will be included in the set of component modification events that are fired.
    ///     Note that direct access does not provide access to any templated values.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if accessed outside the direct access phase.</exception>
    public T[] Components
    {
        get
        {
            if (!enableDirectAccess) throw new InvalidOperationException("Direct access not allowed now.");
            return components;
        }
    }

    /// <summary>
    ///     Indicates the number of components that need to be iterated during direct access.
    /// </summary>
    public int ComponentCount => nextIndex;

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
    public T this[ulong entityId]
    {
        get
        {
            if (entityToComponentMap.TryGetValue(entityId, out var index)) return components[index];
            if (entityTable.TryGetTemplate(entityId, out var templateId) &&
                entityToComponentMap.TryGetValue(templateId, out var templateIndex))
                return components[templateIndex];
            throw new KeyNotFoundException($"No component or template component for entity {entityId:X}.");
        }
    }

    public event Action? OnStartUpdates;

    public event ComponentEventDelegates<T>.ComponentAddedEventHandler? OnComponentAdded;

    public event ComponentEventDelegates<T>.ComponentRemovedEventHandler? OnComponentRemoved;

    public event ComponentEventDelegates<T>.ComponentModifiedEventHandler? OnComponentModified;

    public event Action? OnEndUpdates;

    /// <summary>
    ///     Fully removes a component from the collection.
    /// </summary>
    /// <param name="entityId">Entity ID whose component is to be removed.</param>
    /// <param name="isUnload">If true, treat this as an unload rather than a full remove.</param>
    public void RemoveComponent(ulong entityId, bool isUnload = false)
    {
        /* Ensure that a component is directly associated. */
        if (!entityToComponentMap.ContainsKey(entityId)) return;

        /* Enqueue the removal. */
        var pendingRemove = new PendingRemove
        {
            EntityId = entityId,
            IsUnload = isUnload
        };
        pendingRemoves.Add(ref pendingRemove);

        hasRemoves = true;
    }

    /// <summary>
    ///     Applies pending component updates.
    /// </summary>
    public void ApplyComponentUpdates()
    {
        /* Apply all pending operations. */
        if (hasReclaims) ApplyReclaims();
        if (hasAdds) ApplyAdds();
        if (hasModifications) ApplyModifications();
        if (hasRemoves) ApplyRemoves();

        /* Dispatch events as needed. */
        FireComponentEvents();
    }

    /// <summary>
    ///     Event triggered when the direct access phase of the component update process begins.
    ///     Each component collection should have at most one subscriber to this event.
    ///     First parameter is an array of modified component indices to append to.
    ///     Return value is the number of indices added during this call.
    /// </summary>
    public event Func<int[], int>? OnBeginDirectAccess;

    /// <summary>
    ///     Enqueues the creation of a new component associated with the given entity.
    ///     If a component of this type is already associated with the entity, the
    ///     existing component will be updated to the given initial value.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="initialValue">Initial value of the component.</param>
    /// <param name="isLoad">If true, treat the add as a load.</param>
    /// <exception cref="NotSupportedException">
    ///     Thrown if a component is already associated with the entity, and the
    ///     Set operation is not supported.
    /// </exception>
    public void AddComponent(ulong entityId, T initialValue, bool isLoad = false)
    {
        if (entityToComponentMap.ContainsKey(entityId))
        {
            /* Component already exists - enqueue an update. */
            ModifyComponent(entityId, ComponentOperation.Set, initialValue);
        }
        else
        {
            /* Component does not exist - enqueue an add. */
            var pendingAdd = new PendingAdd
            {
                EntityId = entityId,
                InitialValue = initialValue,
                IsLoad = isLoad
            };
            pendingAdds.Add(ref pendingAdd);
            pendingAddEntityIds.Add(entityId);

            hasAdds = true;
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
        if (!entityToComponentMap.TryGetValue(entityId, out var componentIndex)) return;

        /* Ensure that the operation is supported by this component. */
        if (!operators.ContainsKey(operation))
            throw new NotSupportedException();

        /* Enqueue a modification. */
        var pendingModify = new PendingModify
        {
            EntityId = entityId,
            ComponentIndex = componentIndex,
            ComponentOperation = operation,
            Adjustment = adjustment
        };
        pendingModifications[operation].Add(ref pendingModify);

        hasModifications = true;
    }

    /// <summary>
    ///     Convenience method that adds a new component or replaces its value if it already exists.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="newValue">New value.</param>
    /// <param name="isLoad">For adds, whether to treat as a load rather than an add.</param>
    public void AddOrUpdateComponent(ulong entityId, T newValue, bool isLoad = false)
    {
        if (entityToComponentMap.ContainsKey(entityId))
            ModifyComponent(entityId, ComponentOperation.Set, newValue);
        else
            AddComponent(entityId, newValue, isLoad);
    }

    /// <summary>
    ///     Determines whether a component is associated with the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">Whether to consider newly removed components in the same tick.</param>
    /// <returns>true if a component is associated, false otherwise.</returns>
    public bool HasComponentForEntity(ulong entityId, bool lookback = false)
    {
        var hasLocal = HasLocalComponentForEntity(entityId, lookback);
        return hasLocal || (entityTable.TryGetTemplate(entityId, out var templateEntityId) &&
                            HasComponentForEntity(templateEntityId));
    }

    /// <summary>
    ///     Determines whether a component is explicitly associated with the given entity (i.e. not
    ///     through a template entity inherited by the entity).
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="lookback">Whether to consider newly removed components in the same tick.</param>
    /// <returns>true if a local component is associated, false otherwise.</returns>
    public bool HasLocalComponentForEntity(ulong entityId, bool lookback = false)
    {
        return entityToComponentMap.ContainsKey(entityId) && (lookback || !pendingReclaims.Contains(entityId));
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
    /// <param name="lookback">If true, get the value of a removed or unloaded component that has not been reclaimed.</param>
    /// <returns>Component associated with the given entity.</returns>
    /// <remarks>
    ///     When lookback is true, this method will retrieve the value of the component if it has been removed or
    ///     unloaded during the previous tick transition. This one-tick delay to resource reclaiming allows for
    ///     prior values to be inspected during update processing, e.g. in a remove or unload delegate function.
    /// </remarks>
    public Maybe<T> GetComponentForEntity(ulong entityId, bool lookback = false)
    {
        var maybe = new Maybe<T>();
        if ((lookback || !pendingReclaims.Contains(entityId))
            && entityToComponentMap.TryGetValue(entityId, out var componentId))
            maybe.Value = components[componentId];
        if (!maybe.HasValue && entityTable.TryGetTemplate(entityId, out var templateEntityId))
            maybe = GetComponentForEntity(templateEntityId, lookback);
        return maybe;
    }

    /// <summary>
    ///     Gets a component, even if it was removed in the last tick.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <returns>Component value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no value is associated to the entity.</exception>
    public T GetComponentWithLookback(ulong entityId)
    {
        if (entityToComponentMap.TryGetValue(entityId, out var componentId))
            return components[componentId];
        if (entityTable.TryGetTemplate(entityId, out var templateEntityId) &&
            entityToComponentMap.TryGetValue(templateEntityId, out var templateComponentId))
            return components[templateComponentId];

        throw new KeyNotFoundException("No component for entity");
    }

    /// <summary>
    ///     Creates a dictionary mapping entity IDs to all known components.
    /// </summary>
    /// <remarks>
    ///     This method should be used sparingly - it requires allocation and creation of
    ///     an entire dictionary. It is intended to be used in the initial creation of
    ///     a ComponentIndexer. Consider using GetComponentForEntity() in combination
    ///     with an appropriate ComponentIndexer instead.
    ///     As with direct access, templated values are ignored by this method.
    /// </remarks>
    /// <returns>Dictionary mapping entity IDs to all known components.</returns>
    public IDictionary<ulong, T> GetAllComponents()
    {
        var dict = new Dictionary<ulong, T>();
        foreach (var entityId in entityToComponentMap.Keys) dict[entityId] = components[entityToComponentMap[entityId]];
        return dict;
    }

    /// <summary>
    ///     Clears the contents of the component collection.
    /// </summary>
    public void Clear()
    {
        entityToComponentMap.Clear();
        indexQueue.Clear();
        nextIndex = 0;
    }

    /// <summary>
    ///     Gets the component value (if any) for the entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Component value. Only meaningful if this method returns true.</param>
    /// <returns>true if a component was found, false otherwise.</returns>
    public bool TryGetValue(ulong entityId, out T value)
    {
        var hasValue = HasComponentForEntity(entityId);
        value = hasValue ? this[entityId] : default!;
        return hasValue;
    }

    /// <summary>
    ///     Attempts to find the "nearest" component value to the given entity by traversing its hierarchy
    ///     upward, taking the first direct or templated value encountered (if any).
    /// </summary>
    /// <param name="entityId">Entity ID at which to begin search.</param>
    /// <param name="parentCollection">Component collection mapping entities to their parent entities.</param>
    /// <param name="nearestValue">Nearest component value. Only meaningful if this method returns true.</param>
    /// <param name="owningEntityId">
    ///     Entity ID of the entity with the matching component. Only meaningful if this method
    ///     returns true.
    /// </param>
    /// <returns>true if a component value was found, false otherwise.</returns>
    public bool TryFindNearest(ulong entityId, BaseComponentCollection<ulong> parentCollection, out T nearestValue,
        out ulong owningEntityId)
    {
        var current = entityId;
        do
        {
            if (HasComponentForEntity(current))
            {
                nearestValue = this[current];
                owningEntityId = current;
                return true;
            }
        } while (parentCollection.TryGetValue(current, out current));

        // If we get here, we didn't find anything in the hierarchy.
        nearestValue = default!;
        owningEntityId = 0;
        return false;
    }

    /// <summary>
    ///     Gets the entity ID associated with the given direct index, if any.
    /// </summary>
    /// <param name="index">Direct index.</param>
    /// <param name="entityId">Entity ID. Only valid if the method returns true.</param>
    /// <returns>true if the direct index in use, false otherwise.</returns>
    public bool TryGetEntityForIndex(int index, out ulong entityId)
    {
        entityId = componentToEntityMap[index];
        return entityId > 0;
    }

    /// <summary>
    ///     Gets the component index associated with the given entity ID, if any.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="index">Component index. Only valid if the method returns true.</param>
    /// <returns>true if the entity has a component, false otherwise.</returns>
    public bool TryGetIndexForEntity(ulong entityId, out int index)
    {
        return entityToComponentMap.TryGetValue(entityId, out index);
    }

    /// <summary>
    ///     Fires all pending component events.
    /// </summary>
    private void FireComponentEvents()
    {
        /* Announce that events are being fired. */
        OnStartUpdates?.Invoke();

        // Open the collection to direct iteration by systems for bulk processing.
        enableDirectAccess = true;
        var directModCount = OnBeginDirectAccess?.Invoke(directAccessModifiedIndices) ?? 0;
        enableDirectAccess = false;

        /* Fire events. */
        FireAddAndLoadEvents();
        FireModificationEvents(directModCount);
        FireRemoveAndUnloadEvents();

        /* Announce that events are done being fired. */
        OnEndUpdates?.Invoke();
    }

    /// <summary>
    ///     Applies all pending component reclaims.
    /// </summary>
    private void ApplyReclaims()
    {
        foreach (var entityId in pendingReclaims) ApplyReclaim(entityId);
        pendingReclaims.Clear();
        hasReclaims = false;
    }

    /// <summary>
    ///     Applies all pending component additions.
    /// </summary>
    private void ApplyAdds()
    {
        /* Iterate over new components. */
        for (var i = 0; i < pendingAdds.Count; ++i)
        {
            ref var pendingAdd = ref pendingAdds[i];
            ApplyAddComponent(pendingAdd.EntityId, pendingAdd.InitialValue);
            if (pendingAdd.IsLoad)
                pendingLoadEvents.Add(pendingAdd.EntityId);
            else
                pendingAddEvents.Add(pendingAdd.EntityId);
        }

        /* Reset the buffer. */
        pendingAddEntityIds.Clear();
        pendingAdds.Clear();
        hasAdds = false;
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
            for (var i = 0; i < queue.Count; ++i)
            {
                ref var pendingModify = ref queue[i];
                var transformed = op(components[pendingModify.ComponentIndex],
                    pendingModify.Adjustment);
                components[pendingModify.ComponentIndex] = transformed;
                pendingModifyEvents.Add(pendingModify.EntityId);
            }

            /* Reset the buffer. */
            queue.Clear();
        }

        hasModifications = false;
    }

    /// <summary>
    ///     Applies all pending component removals.
    /// </summary>
    private void ApplyRemoves()
    {
        /* Apply operations. */
        for (var i = 0; i < pendingRemoves.Count; ++i)
        {
            ref var pendingRemove = ref pendingRemoves[i];
            ApplyRemoveOrUnloadComponent(pendingRemove.EntityId, pendingRemove.IsUnload);
        }

        /* Clear the buffer. */
        pendingRemoves.Clear();
        hasRemoves = false;
    }

    /// <summary>
    ///     Fires events for component additions.
    /// </summary>
    private void FireAddAndLoadEvents()
    {
        if (OnComponentAdded != null)
        {
            // Adds.
            foreach (var entityId in pendingAddEvents)
            {
                if (!HasLocalComponentForEntity(entityId)) continue;
                var value = this[entityId];
                OnComponentAdded.Invoke(entityId, value, false);
            }

            // Loads.
            foreach (var entityId in pendingLoadEvents)
            {
                if (!HasLocalComponentForEntity(entityId)) continue;
                var value = this[entityId];
                OnComponentAdded.Invoke(entityId, value, true);
            }
        }

        /* Reset the pending events. */
        pendingAddEvents.Clear();
        pendingLoadEvents.Clear();
    }

    /// <summary>
    ///     Fires events for component modifications.
    /// </summary>
    /// <param name="directModCount">Number of direct modifications.</param>
    private void FireModificationEvents(int directModCount)
    {
        /* Iterate over entities that have been modified. */
        if (OnComponentModified != null)
        {
            foreach (var entityId in pendingModifyEvents)
            {
                /* Notify all listeners. */
                if (!HasLocalComponentForEntity(entityId)) continue;
                var value = this[entityId];
                OnComponentModified.Invoke(entityId, value);
            }

            for (var i = 0; i < directModCount; ++i)
            {
                var entityId = componentToEntityMap[directAccessModifiedIndices[i]];
                if (!HasLocalComponentForEntity(entityId)) continue;
                var value = this[entityId];
                OnComponentModified.Invoke(entityId, value);
            }
        }

        /* Reset the pending events. */
        pendingModifyEvents.Clear();
    }

    /// <summary>
    ///     Fires events for component removals.
    /// </summary>
    private void FireRemoveAndUnloadEvents()
    {
        if (OnComponentRemoved != null)
        {
            // Removes.
            foreach (var entityId in pendingRemoveEvents) OnComponentRemoved.Invoke(entityId, false);

            // Unloads.
            foreach (var entityId in pendingUnloadEvents) OnComponentRemoved.Invoke(entityId, true);
        }

        /* Reset the pending events. */
        pendingRemoveEvents.Clear();
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
    ///     Removes/unloads the given component, keeping it in memory for one additional tick so that
    ///     the value may be referenced from code triggered by remove/unload delegates.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="isUnload">If true, treat as unload.</param>
    private void ApplyRemoveOrUnloadComponent(ulong entityId, bool isUnload)
    {
        if (!entityToComponentMap.ContainsKey(entityId)) return;
        if (isUnload)
            pendingUnloadEvents.Add(entityId);
        else
            pendingRemoveEvents.Add(entityId);

        componentToEntityMap[entityToComponentMap[entityId]] = 0;
        pendingReclaims.Add(entityId);
        hasReclaims = true;
    }

    /// <summary>
    ///     Immediately removes or unloads the component associated with the given entity.
    ///     If no component is associated, this method has no effect.
    ///     This method should only be used from the main thread.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void ApplyReclaim(ulong entityId)
    {
        /* Ensure that a component is associated to the entity. *default/
        if (!entityToComponentMap.ContainsKey(entityId)) return;

        /* Remove the component, but leave it allocated for later reuse. */
        var index = entityToComponentMap[entityId];
        components[index] = default!;
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
        // Select the next index.
        int index;
        if (indexQueue.Count > 0)
            /* Reuse a previously deleted component. */
            index = indexQueue.Dequeue();
        else
            /* Append to the next position. */
            index = nextIndex++;

        // If the index is past the end of the backing array, resize.
        if (index >= components.Length)
        {
            var newComponents = new T[components.Length + resizeIncrement];
            Array.Copy(components, newComponents, components.Length);
            components = newComponents;

            var newComponentMap = new ulong[components.Length];
            Array.Copy(componentToEntityMap, newComponentMap, components.Length);
            componentToEntityMap = newComponentMap;

            directAccessModifiedIndices = new int[components.Length];
        }

        // Insert the component.
        components[index] = componentValue;

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
        ///     Affected entity ID.
        /// </summary>
        public ulong EntityId;

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

        /// <summary>
        ///     Whether this add is a load.
        /// </summary>
        public bool IsLoad;
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

        /// <summary>
        ///     Whether this remove is an unload.
        /// </summary>
        public bool IsUnload;
    }
}