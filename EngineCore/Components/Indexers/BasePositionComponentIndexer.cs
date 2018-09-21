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
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sovereign.EngineUtil.Collections.Octree;

namespace Sovereign.EngineCore.Components.Indexers
{

    /// <summary>
    /// Manages a position-based index into a ComponentCollection.
    /// </summary>
    ///
    /// Extend this class for each ComponentCollection that requires position indexing.
    public class BasePositionComponentIndexer : IDisposable
    {

        /// <summary>
        /// Component collection.
        /// </summary>
        protected readonly IComponentEventSource<Vector3> componentEventSource;

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
        /// <param name="componennCollection">Component collection.</param>
        /// <param name="componentEventSource">Component event source.</param>
        protected BasePositionComponentIndexer(BaseComponentCollection<Vector3> componennCollection, 
            IComponentEventSource<Vector3> componentEventSource)
        {
            this.componentEventSource = componentEventSource;

            /* Register component callbacks. */
            componentEventSource.OnStartUpdates += OnStartUpdates;
            componentEventSource.OnComponentAdded += OnComponentAdded;
            componentEventSource.OnComponentModified += OnComponentModified;
            componentEventSource.OnComponentRemoved += OnComponentRemoved;
            componentEventSource.OnEndUpdates += OnEndUpdates;

            /* Create the initial index. */
            octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, 
                componennCollection.GetAllComponents());
        }

        public void Dispose()
        {
            /* Deregister callbacks. */
            componentEventSource.OnStartUpdates -= OnStartUpdates;
            componentEventSource.OnComponentAdded -= OnComponentAdded;
            componentEventSource.OnComponentModified -= OnComponentModified;
            componentEventSource.OnComponentRemoved -= OnComponentRemoved;
            componentEventSource.OnEndUpdates -= OnEndUpdates;

            /* Release the update lock if needed. */
            if (updateLock != null)
                updateLock.Dispose();
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
            Vector3 maxPosition, IList<Tuple<Vector3, ulong>> buffer)
        {
            octree.GetElementsInRange(indexerLock.octreeLock, minPosition, maxPosition, buffer);
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

        /// <summary>
        /// Called when the component collection begins updating values.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="args">Unused.</param>
        private void OnStartUpdates(object sender, EventArgs args)
        {
            updateLock = AcquireLock();
        }

        /// <summary>
        /// Called when a component is added.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">Initial position.</param>
        private void OnComponentAdded(ulong entityId, Vector3 position)
        {
            octree.Add(updateLock.octreeLock, position, entityId);
        }

        /// <summary>
        /// Called when a component is modified.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">New position.</param>
        private void OnComponentModified(ulong entityId, Vector3 position)
        {
            octree.UpdatePosition(updateLock.octreeLock, entityId, position);
        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        private void OnComponentRemoved(ulong entityId)
        {
            octree.Remove(updateLock.octreeLock, entityId);
        }

        /// <summary>
        /// Called when the component collection has finished updates.
        /// </summary>
        /// <param name="source">Unused.</param>
        /// <param name="args">Unused.</param>
        private void OnEndUpdates(object source, EventArgs args)
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
