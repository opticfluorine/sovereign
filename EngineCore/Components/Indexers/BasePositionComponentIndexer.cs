using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Engine8.EngineUtil.Collections.Octree;

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
            octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, 
                componennCollection.GetAllComponents());
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
        /// <param name="buffer">Buffer to which the results will be appended.</param>
        public void GetEntitiesInRange(Vector<float> minPosition, Vector<float> maxPosition,
            IList<Tuple<Vector<float>, ulong>> buffer)
        {
            octree.GetElementsInRange(minPosition, maxPosition, buffer);
        }

        /// <summary>
        /// Called when a component is added.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">Initial position.</param>
        private void OnComponentAdded(ulong entityId, Vector<float> position)
        {
            octree.Add(position, entityId);
        }

        /// <summary>
        /// Called when a component is modified.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">New position.</param>
        private void OnComponentModified(ulong entityId, Vector<float> position)
        {
            octree.UpdatePosition(entityId, position);
        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        private void OnComponentRemoved(ulong entityId)
        {
            octree.Remove(entityId);
        }

    }

}
