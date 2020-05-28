/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Sovereign.EngineUtil.Collections.Octree;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Manages a position-based index into a ComponentCollection.
    /// </summary>
    ///
    /// Extend this class for each ComponentCollection that requires position indexing.
    public class BasePositionComponentIndexer : BaseComponentIndexer<Vector3>
    {

        /// <summary>
        /// Octree providing partitioning to the set of tracked entity IDs.
        /// </summary>
        private readonly Octree<ulong> octree;

        /// <summary>
        /// Lock acquired while the collection is performing updates.
        /// </summary>
        private IndexerLock updateLock;

        /// <summary>
        /// Creates an indexer for the given component collection.
        /// </summary>
        /// <param name="components">Component collection.</param>
        /// <param name="componentEventSource">Component event source.</param>
        protected BasePositionComponentIndexer(BaseComponentCollection<Vector3> components,
            IComponentEventSource<Vector3> componentEventSource)
            : base(components, componentEventSource)
        {
            /* Create the initial index. */
            octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin,
                components.GetAllComponents());
        }

        /// <summary>
        /// Finds all entities with position components in the box with minPosition at
        /// the bottom-left corner and maxPosition at the top-right corner.
        /// </summary>
        /// <param name="indexerLock">Indexer lock.</param>
        /// <param name="minPosition">Bottom-left corner of the search space.</param>
        /// <param name="maxPosition">Top-right corner of the search space.</param>
        /// <param name="buffer">Buffer to which the results will be appended.</param>
        public void GetEntitiesInRange(IndexerLock indexerLock, Vector3 minPosition,
            Vector3 maxPosition, IList<PositionedEntity> buffer)
        {
            octree.GetElementsInRange(indexerLock.octreeLock, minPosition, maxPosition, buffer,
                (position, id) => new PositionedEntity()
                {
                    Position = position,
                    EntityId = id
                });
        }

        /// <summary>
        /// Blocks until an indexer lock can be acquired.
        /// </summary>
        /// <returns>Active lock handle.</returns>
        public IndexerLock AcquireLock()
        {
            return new IndexerLock(this);
        }

        /// <summary>
        /// Attempts to obtain an indexer lock.
        /// </summary>
        /// <param name="indexerLock">Indexer lock. Undefined if the method returns false.</param>
        /// <returns>true if the lock was acquired, false otherwise.</returns>
        public bool TryAcquireLock(out IndexerLock indexerLock)
        {
            /* Attempt to acquire a lock on the octree. */
            var acquired = octree.TryAcquireLock(out var octreeLock);
            indexerLock = acquired ? new IndexerLock(this, octreeLock) : null;
            return acquired;
        }

        protected override void StartUpdatesCallback(object sender, EventArgs args)
        {
            updateLock = AcquireLock();
        }

        protected override void ComponentAddedCallback(ulong entityId, Vector3 position)
        {
            octree.Add(updateLock.octreeLock, position, entityId);
        }

        protected override void ComponentModifiedCallback(ulong entityId, Vector3 position)
        {
            octree.UpdatePosition(updateLock.octreeLock, entityId, position);
        }

        protected override void ComponentRemovedCallback(ulong entityId)
        {
            octree.Remove(updateLock.octreeLock, entityId);
        }

        protected override void ComponentUnloadedCallback(ulong entityId)
        {
            octree.Remove(updateLock.octreeLock, entityId);
        }

        protected override void EndUpdatesCallback(object source, EventArgs args)
        {
            /* Release the update lock. */
            updateLock.Dispose();
            updateLock = null;
        }

        /// <summary>
        /// Lock handle for the indexer.
        /// </summary>
        public class IndexerLock : IDisposable
        {

            /// <summary>
            /// Inner lock.
            /// </summary>
            internal readonly Octree<ulong>.OctreeLock octreeLock;

            public IndexerLock(BasePositionComponentIndexer indexer)
                : this(indexer, indexer.octree.AcquireLock())
            {

            }

            public IndexerLock(BasePositionComponentIndexer indexer, Octree<ulong>.OctreeLock octreeLock)
            {
                this.octreeLock = octreeLock;
            }

            public void Dispose()
            {
                octreeLock.Dispose();
            }

        }

    }

}
