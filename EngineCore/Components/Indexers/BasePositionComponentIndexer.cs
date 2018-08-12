using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Engine8.EngineUtil.Collections;

namespace Engine8.EngineCore.Components.Indexers
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
        protected readonly IComponentEventSource<Vector<float>> componentEventSource;

        /// <summary>
        /// Octree providing partitioning to the set of tracked entity IDs.
        /// </summary>
        private readonly Octree<ulong> octree;
        
        /// <summary>
        /// Creates an indexer for the given component collection.
        /// </summary>
        /// <param name="componennCollection">Component collection.</param>
        /// <param name="componentEventSource">Component event source.</param>
        protected BasePositionComponentIndexer(BaseComponentCollection<Vector<float>> componennCollection, 
            IComponentEventSource<Vector<float>> componentEventSource)
        {
            this.componentEventSource = componentEventSource;

            /* Register component callbacks. */
            componentEventSource.OnComponentAdded += OnComponentAdded;
            componentEventSource.OnComponentModified += OnComponentModified;
            componentEventSource.OnComponentRemoved += OnComponentRemoved;

            /* Create the initial index. */
            octree = new Octree<ulong>(componennCollection.GetAllComponents());
        }

        public void Dispose()
        {
            /* Deregister callbacks. */
            componentEventSource.OnComponentAdded -= OnComponentAdded;
            componentEventSource.OnComponentModified -= OnComponentModified;
            componentEventSource.OnComponentRemoved -= OnComponentRemoved;
        }

        /// <summary>
        /// Finds all entities with position components in the box with minPosition at
        /// the bottom-left corner and maxPosition at the top-right corner.
        /// </summary>
        /// <param name="minPosition">Bottom-left corner of the search space.</param>
        /// <param name="maxPosition">Top-right corner of the search space.</param>
        /// <returns>All entities with position components in the given range.</returns>
        public IEnumerable<ulong> GetEntitiesInRange(Vector<float> minPosition, Vector<float> maxPosition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all entities in the given range and bins them by the z component of their position.
        /// </summary>
        /// <param name="minPosition">Bottom-left corner of the search space.</param>
        /// <param name="maxPosition">Top-right corner of the search space.</param>
        /// <param name="binSize">Interval on which bins are created.</param>
        ///
        /// The bin boundaries are a sequence 0, binSize, 2 * binSize, ..., n * binSize. Each bin has
        /// inclusive lower bound and exclusive upper bound.
        /// 
        /// <returns>Dictionary mapping the floor of each bin to the entities in the bin.</returns>
        public IDictionary<float, IEnumerable<ulong>> GetEntitiesInRangeZBinned(Vector<float> minPosition,
            Vector<float> maxPosition, float binSize = 1.0f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when a component is added.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void OnComponentAdded(Object sender,
            ComponentAddModifyEventArgs<Vector<float>> eventArgs)
        {

        }

        /// <summary>
        /// Called when a component is modified.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void OnComponentModified(Object sender,
            ComponentAddModifyEventArgs<Vector<float>> eventArgs)
        {

        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="sender">Object that sent the event.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void OnComponentRemoved(Object sender,
            ComponentRemoveEventArgs eventArgs)
        {

        }

    }

}
