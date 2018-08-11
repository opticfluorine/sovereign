using System;
using System.Collections.Generic;
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
    public class PositionIndexer : IDisposable
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
        /// <param name="componentEventSource">Component event source.</param>
        protected PositionIndexer(IComponentEventSource<Vector<float>> componentEventSource)
        {
            this.componentEventSource = componentEventSource;

            /* Register component callbacks. */
            componentEventSource.OnComponentAdded += OnComponentAdded;
            componentEventSource.OnComponentModified += OnComponentModified;
            componentEventSource.OnComponentRemoved += OnComponentRemoved;

            /* Create the initial index. */
        }

        public void Dispose()
        {
            /* Deregister callbacks. */
            componentEventSource.OnComponentAdded -= OnComponentAdded;
            componentEventSource.OnComponentModified -= OnComponentModified;
            componentEventSource.OnComponentRemoved -= OnComponentRemoved;
        }

        /// <summary>
        /// Called when a component is added.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">Initial position.</param>
        private void OnComponentAdded(ulong entityId, Vector<float> position)
        {

        }

        /// <summary>
        /// Called when a component is modified.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="position">New position.</param>
        private void OnComponentModified(ulong entityId, Vector<float> position)
        {

        }

        /// <summary>
        /// Called when a component is removed.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        private void OnComponentRemoved(ulong entityId)
        {

        }

    }

}
