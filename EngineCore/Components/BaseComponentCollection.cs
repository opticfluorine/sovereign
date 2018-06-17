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
    /// <typeparam name="T">Component value type.</typeparam>
    public class BaseComponentCollection<T>
    {

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

        protected BaseComponentCollection(int initialSize)
        {
            components = new List<T>(initialSize);
        }

        /// <summary>
        /// Provides access to the components indexed by the associated
        /// enttiy ID.
        /// 
        /// The setter should only be invoked from the main thread.
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

            set
            {
                if (HasComponentForEntity(entityId))
                {
                    components[entityToComponentMap[entityId]] = value;
                }
                else
                {
                    AddComponent(entityId, value);
                }
            }
        }

        /// <summary>
        /// Removes the component associated with the given entity.
        /// If no component is associated, this method has no effect.
        /// 
        /// This method should only be used from the main thread.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void RemoveComponent(ulong entityId)
        {
            /* Ensure that a component is associated to the entity. */
            if (!entityToComponentMap.ContainsKey(entityId)) return;

            /* Remove the component, but leave it allocated for later reuse. */
            var index = entityToComponentMap[entityId];
            entityToComponentMap.Remove(entityId);
            indexQueue.Enqueue(index);
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

        }

        /// <summary>
        /// Adds a component for the given entity.
        /// 
        /// This method should only be used from the main thread.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">Initial value of the new component.</param>
        private void AddComponent(ulong entityId, T componentValue)
        {
            /* Insert the component into the buffer. */
            var index = AddComponentToBuffer(componentValue);

            /* Create the appropriate indices into the buffer. */
            RegisterIndices(index, entityId);
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

    }


}
