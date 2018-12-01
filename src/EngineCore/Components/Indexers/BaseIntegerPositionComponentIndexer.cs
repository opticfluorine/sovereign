/*
 * Sovereign Engine
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

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Base class for component indexers that allow lookup by exact integer
    /// grid coordinates.
    /// </summary>
    public class BaseIntegerPositionComponentIndexer : BaseComponentIndexer<Vector3>
    {

        private readonly IDictionary<GridPosition, ISet<ulong>> entitiesByPosition
            = new Dictionary<GridPosition, ISet<ulong>>();

        private readonly IDictionary<ulong, GridPosition> knownPositions
            = new Dictionary<ulong, GridPosition>();

        protected BaseIntegerPositionComponentIndexer(BaseComponentCollection<Vector3> components,
            IComponentEventSource<Vector3> eventSource)
            : base(components, eventSource)
        {
        }

        /// <summary>
        /// Gets the entities at the given grid position.
        /// </summary>
        /// <param name="position">Position to query.</param>
        /// <returns>The set of entities at the position, or null if there are none.</returns>
        public ISet<ulong> GetEntitiesAtPosition(GridPosition position)
        {
            return entitiesByPosition.TryGetValue(position, out var entities)
                ? entities : null;
        }

        protected override void OnComponentAdded(ulong entityId, Vector3 componentValue)
        {
            AddEntity(entityId, componentValue);
        }

        protected override void OnComponentModified(ulong entityId, Vector3 componentValue)
        {
            RemoveEntity(entityId);
            AddEntity(entityId, componentValue);
        }

        protected override void OnComponentRemoved(ulong entityId)
        {
            RemoveEntity(entityId);
        }

        /// <summary>
        /// Adds the given entity to the index.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">Position.</param>
        private void AddEntity(ulong entityId, Vector3 position)
        {
            var gridPos = new GridPosition(position);
            var set = GetSetForPosition(gridPos);
            set.Add(entityId);
        }

        /// <summary>
        /// Gets the set for the given grid position, creating it if necessary.
        /// </summary>
        /// <param name="position">Grid position.</param>
        /// <returns>Set.</returns>
        private ISet<ulong> GetSetForPosition(GridPosition position)
        {
            var exists = entitiesByPosition.TryGetValue(position, out var set);
            if (!exists)
            {
                set = new HashSet<ulong>();
                entitiesByPosition[position] = set;
            }
            return set;
        }

        /// <summary>
        /// Removes the given entity from the index.
        /// </summary>
        /// <param name="entityId">Entity ID to remove.</param>
        private void RemoveEntity(ulong entityId)
        {
            if (!knownPositions.ContainsKey(entityId)) return;
            var gridPos = knownPositions[entityId];
            var set = entitiesByPosition[gridPos];

            set.Remove(entityId);
            knownPositions.Remove(entityId);
        }

    }

}
