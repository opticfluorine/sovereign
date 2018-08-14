using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Engine8.EngineUtil.Collections.Octree
{

    /// <summary>
    /// Octree implementation for managing mutable sets of
    /// distinct objects in three-dimensional space.
    /// </summary>
    ///
    /// The Octree class is thread-safe.
    public class Octree<T>
    {
        
        /// <summary>
        /// Default number of levels to initially generate in the tree.
        /// </summary>
        public const int DefaultInitialLevels = 5;

        /// <summary>
        /// Default minimum length of a node.
        /// </summary>
        public const float DefaultMinimumNodeSize = 1.0f;

        /// <summary>
        /// Default origin vector.
        /// </summary>
        public static readonly Vector<float> DefaultOrigin = new Vector<float>(0.0f);

        /// <summary>
        /// Minimum length of a node.
        /// </summary>
        public readonly float MinimumNodeSize;

        /// <summary>
        /// Root node of the octree.
        /// </summary>
        private OctreeNode<T> rootNode;

        /// <summary>
        /// Creates an empty octree.
        /// </summary>
        /// <param name="origin">Center point of the initial tree.</param>
        /// <param name="initialLevels">Number of levels to initially generate in the tree.</param>
        /// <param name="minimumNodeSize">Minimum length of a node.</param>
        public Octree(Vector<float> origin,
            int initialLevels = DefaultInitialLevels, 
            float minimumNodeSize = DefaultMinimumNodeSize)
        {
            MinimumNodeSize = minimumNodeSize;

            /* Create the initial node. */
            var halfRootNodeLength = 0.5f * (float)Math.Pow(minimumNodeSize, initialLevels);
            var span = new Vector<float>(halfRootNodeLength);
            var minPosition = Vector.Subtract(origin, span);
            var maxPosition = Vector.Add(origin, span);
             
            rootNode = new OctreeNode<T>(this, minPosition, maxPosition);
        }

        /// <summary>
        /// Creates an octree containing the given objects.
        /// </summary>
        /// <param name="origin">Center point of the initial tree.</param>
        /// <param name="initialData">Initial elements to be added to the octree.</param>
        /// <param name="initialLevels">Number of levels to initially generate in the tree.</param>
        /// <param name="minimumNodeSize">Minimum length of a node.</param>
        public Octree(Vector<float> origin,
            IDictionary<T, Vector<float>> initialData,
            int initialLevels = DefaultInitialLevels,
            float minimumNodeSize = DefaultMinimumNodeSize) : this(origin, initialLevels, minimumNodeSize)
        {
            /* Add all initial points to the octree. */
            foreach (var element in initialData.Keys)
            {
                var position = initialData[element];
                Add(position, element);
            }
        }
        
        /// <summary>
        /// Adds the given element to the octree at the given position.
        /// </summary>
        /// 
        /// If the element is already in the octree, this method has no effect.
        /// 
        /// <param name="position">Initial position of the element.</param>
        /// <param name="element">Element to be added.</param>
        public void Add(Vector<float> position, T element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the position of the given element.
        /// </summary>
        /// 
        /// <param name="element">Element to be updated.</param>
        /// <param name="position">New position of the element.</param>
        ///
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the element is not in the octree.
        /// </exception>
        public void UpdatePosition(T element, Vector<float> position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the given element from the octree.
        /// </summary>
        /// 
        /// <param name="element">Element to be removed.</param>
        /// 
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the element is not in the octree.
        /// </exception>
        public void Remove(T element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the elements in the given range.
        /// </summary>
        /// <param name="minPosition">Minimum position (inclusive).</param>
        /// <param name="maxPosition">Maximum position (exclusive).</param>
        /// <param name="buffer">Buffer to hold the results.</param>
        public void GetElementsInRange(Vector<float> minPosition,
            Vector<float> maxPosition, IList<Tuple<Vector<float>, T>> buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rebalances the given leaf node, applying further partitioning as needed.
        /// </summary>
        /// <param name="node">Node to rebalance.</param>
        ///
        /// The node is assumed to be a leaf node.
        private void RebalanceLeafNode(OctreeNode<T> node)
        {
            /* Depth-first search through the leaf nodes. */
            var nodesToVisit = new Stack<OctreeNode<T>>();
            nodesToVisit.Push(node);
            while (nodesToVisit.Count > 0)
            {
                /* Take the next node off the stack. */
                var currentNode = nodesToVisit.Pop();

                /* If there are less than two elements, the node is already balanced. */
                if (currentNode.elementPositions.Count < 2) continue;

                /* 
                 * Otherwise determine whether the rebalance will distribute elements
                 * into separate octants.
                 */
                var elementPositions = currentNode.elementPositions;
                var firstOctant = currentNode.GetOctantForPosition(elementPositions[elementPositions.Keys.First()]);
                var needToRebalance = false;
                foreach (var element in elementPositions.Keys.Skip(1))
                {
                    var nextOctant = currentNode.GetOctantForPosition(elementPositions[element]);
                    needToRebalance = needToRebalance || (nextOctant != firstOctant);
                }
                if (!needToRebalance) continue;

               
            }
        }

    }

}
