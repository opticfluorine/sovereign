/*
 * Engine8 Dynamic World MMORPG Engine
 * Copyright (c) 2018 opticfluorine
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Castle.Core.Logging;
using Engine8.EngineUtil.Monads;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Components
{

    /// <summary>
    /// Base class of component collections.
    /// </summary>
    /// 
    /// Component values are updated asynchronously by enqueuing add, remove, and
    /// modify operations. These operations are executed on the main thread at the
    /// transition between event ticks in the following order:
    /// 
    /// <list type="number">
    /// <item>Add</item>
    /// <item>Modify</item>
    /// <item>Remove</item>
    /// </list>
    /// 
    /// Operations are enqueued with respect to the current state of the component;
    /// for example, RemoveComponent() cannot be called to remove a component that
    /// is currently enqueued for addition.
    /// 
    /// <typeparam name="T">Component value type.</typeparam>
    public class BaseComponentCollection<T>
    {

        public ILogger Log { private get; set; } = NullLogger.Instance;

        /// <summary>
        /// Underlying component array.
        /// </summary>
        private readonly List<T> components;

        /// <summary>
        /// Buffer indices ready for reuse.
        /// </summary>
        private readonly Queue<int> indexQueue = new Queue<int>();

        /// <summary>
        /// Map from entity ID to associated component.
        /// </summary>
        private readonly IDictionary<ulong, int> entityToComponentMap
            = new ConcurrentDictionary<ulong, int>();

        /// <summary>
        /// Map from component buffer index to associated entity ID.
        /// </summary>
        private readonly IDictionary<int, ulong> componentToEntityMap
            = new ConcurrentDictionary<int, ulong>();

        /// <summary>
        /// Operators associated with this component.
        /// </summary>
        private readonly IDictionary<ComponentOperation, Func<T, T, T>> operators;

        /// <summary>
        /// Pending component additions.
        /// </summary>
        private readonly ConcurrentQueue<PendingAdd> pendingAdds 
            = new ConcurrentQueue<PendingAdd>();

        /// <summary>
        /// Pending component modifications binned by operation.
        /// </summary>
        private readonly IDictionary<ComponentOperation, ConcurrentQueue<PendingModify>> 
            pendingModifications = new Dictionary<ComponentOperation, ConcurrentQueue<PendingModify>>();

        /// <summary>
        /// Pending component removals.
        /// </summary>
        private readonly ConcurrentQueue<PendingRemove> pendingRemoves 
            = new ConcurrentQueue<PendingRemove>();

        /// <summary>
        /// Event triggered when a component is added to the collection.
        /// </summary>
        ///
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        public event EventHandler OnComponentAdded;

        /// <summary>
        /// Event triggered when a component is removed from the collection.
        /// </summary>
        ///
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        public event EventHandler OnComponentRemoved;

        /// <summary>
        /// Event triggered when an existing component is updated.
        /// </summary>
        /// 
        /// This is intended for use with data view objects that are updated
        /// on the main thread once component updates for a given tick are complete.
        public event EventHandler OnComponentModified;

        /// <summary>
        /// Creates a base component collection.
        /// </summary>
        /// <param name="initialSize">Initial size of the component buffer.</param>
        /// <param name="operators">Dict of component operators for use in updates.</param>
        protected BaseComponentCollection(int initialSize,
            IDictionary<ComponentOperation, Func<T, T, T>> operators)
        {
            this.operators = operators;
            components = new List<T>(initialSize);

            /* Key up the allowed operators. */
            foreach (var operation in operators.Keys)
            {
                pendingModifications[operation] = new ConcurrentQueue<PendingModify>();
            }
        }

        /// <summary>
        /// Provides access to the components indexed by the associated
        /// enttiy ID.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>Component associated with the given entity.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown by the getter if no component is associated with the entity ID.
        /// </exception>
        public T this[ulong entityId]
        {
            get
            {
                return components[entityToComponentMap[entityId]];
            }
        }

        /// <summary>
        /// Enqueues the creation of a new component associated with the given entity.
        /// If a component of this type is already associated with the entity, the
        /// existing component will be updated to the given initial value.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="initialValue">Initial value of the component.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if a component is already associated with the entity, and the
        /// Set operation is not supported.
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
                var pendingAdd = new PendingAdd()
                {
                    EntityId = entityId,
                    InitialValue = initialValue,
                };
                pendingAdds.Enqueue(pendingAdd);
            }
        }

        /// <summary>
        /// Enqueues the modification of the component associated with the given entity.
        /// </summary>
        /// 
        /// Note that the component must already exist and be associated with the given
        /// entity; it is not sufficient to first make the corresponding call to
        /// AddComponent.
        /// 
        /// <param name="entityId">Entity ID.</param>
        /// <param name="operation">Operation to perform on the component.</param>
        /// <param name="adjustment">Adjustment value.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no component is currently associated with the given entity.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the requested operation is not supported for this component.
        /// </exception>
        public void ModifyComponent(ulong entityId, ComponentOperation operation, T adjustment)
        {
            /* Ensure that the entity has an associated component. */
            var componentIndex = entityToComponentMap[entityId];

            /* Ensure that the operation is supported by this component. */
            if (!operators.Keys.Contains(operation))
                throw new NotSupportedException();

            /* Enqueue a modification. */
            var pendingModify = new PendingModify()
            {
                ComponentIndex = componentIndex,
                ComponentOperation = operation,
                Adjustment = adjustment,
            };
            pendingModifications[operation].Enqueue(pendingModify);
        }

        /// <summary>
        /// Enqueues the removal of the component associated with the given entity ID.
        /// If no entity is associated with the given entity, no action is performed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void RemoveComponent(ulong entityId)
        {
            /* Ensure that a component is associated. */
            if (!HasComponentForEntity(entityId)) return;

            /* Enqueue the removal. */
            var pendingRemove = new PendingRemove()
            {
                EntityId = entityId,
            };
            pendingRemoves.Enqueue(pendingRemove);
        }

        /// <summary>
        /// Determines whether a component is associated with the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>true if a component is associated, false otherwise.</returns>
        public bool HasComponentForEntity(ulong entityId)
        {
            return entityToComponentMap.ContainsKey(entityId);
        }

        /// <summary>
        /// Gets the component associated with the given entity as a Maybe.
        /// 
        /// Note that this method has increased overhead due to the need to
        /// instantiate 
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <returns>Component associated with the given entity.</returns>
        public Maybe<T> GetComponentForEntity(ulong entityId)
        {
            var maybe = new Maybe<T>();
            if (HasComponentForEntity(entityId))
                maybe.Value = components[entityToComponentMap[entityId]];
            return maybe;
        }

        /// <summary>
        /// Applies pending component updates.
        /// </summary>
        public void ApplyComponentUpdates()
        {
            ApplyAdds();
            ApplyModifications();
            ApplyRemoves();
        }

        /// <summary>
        /// Applies all pending component additions.
        /// </summary>
        private void ApplyAdds()
        {
            /* Iterate over new components. */
            while (pendingAdds.TryDequeue(out PendingAdd pendingAdd))
            {
                ApplyAddComponent(pendingAdd.EntityId, pendingAdd.InitialValue);
            }
        }

        /// <summary>
        /// Applies all pending component modifications.
        /// </summary>
        private void ApplyModifications()
        {
            /* Iterate over operations. */
            foreach (var operation in pendingModifications.Keys)
            {
                /* Transform and update all components. */
                var op = operators[operation];
                var queue = pendingModifications[operation];
                while (queue.TryDequeue(out PendingModify pendingUpdate))
                {
                    var transformed = op(components[pendingUpdate.ComponentIndex],
                        pendingUpdate.Adjustment);
                    components[pendingUpdate.ComponentIndex] = transformed;
                }
            }
        }

        /// <summary>
        /// Applies all pending component removals.
        /// </summary>
        private void ApplyRemoves()
        {
            while (pendingRemoves.TryDequeue(out PendingRemove pendingRemove))
            {
                ApplyRemoveComponent(pendingRemove.EntityId);
            }
        }

        /// <summary>
        /// Immediately adds a component for the given entity.
        /// 
        /// This method should only be used from the main thread.
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
        /// Immediately removes the component associated with the given entity.
        /// If no component is associated, this method has no effect.
        /// 
        /// This method should only be used from the main thread.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        private void ApplyRemoveComponent(ulong entityId)
        {
            /* Ensure that a component is associated to the entity. */
            if (!entityToComponentMap.ContainsKey(entityId)) return;

            /* Remove the component, but leave it allocated for later reuse. */
            var index = entityToComponentMap[entityId];
            entityToComponentMap.Remove(entityId);
            indexQueue.Enqueue(index);
        }

        /// <summary>
        /// Adds a component to the buffer.
        /// </summary>
        /// <param name="componentValue">Component value.</param>
        /// <returns>
        /// Index into the component buffer at which the component
        /// is located.
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
        /// Registers the appropriate lookup indices for the given
        /// component.
        /// </summary>
        /// <param name="bufferIndex">Buffer index of the component.</param>
        /// <param name="entityId">ID of the associated entity.</param>
        private void RegisterIndices(int bufferIndex, ulong entityId)
        {
            entityToComponentMap[entityId] = bufferIndex;
            componentToEntityMap[bufferIndex] = entityId;
        }

        /// <summary>
        /// Describes a pending component modification.
        /// </summary>
        private struct PendingModify
        {
            /// <summary>
            /// Component index for update.
            /// </summary>
            public int ComponentIndex;

            /// <summary>
            /// Operation to perform on the component.
            /// </summary>
            public ComponentOperation ComponentOperation;

            /// <summary>
            /// Adjustment to the component value.
            /// </summary>
            public T Adjustment;
        }

        /// <summary>
        /// Describes a pending component creation.
        /// </summary>
        private struct PendingAdd
        {
            /// <summary>
            /// ID of the associated entity.
            /// </summary>
            public ulong EntityId;

            /// <summary>
            /// Initial value of the new component.
            /// </summary>
            public T InitialValue;
        }

        /// <summary>
        /// Describes a pending component removal.
        /// </summary>
        private struct PendingRemove
        {
            /// <summary>
            /// ID of the associated entity.
            /// </summary>
            public ulong EntityId;
        }

    }


}
