using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Engine8.EngineUtil.Collections.Octree
{

    /// <summary>
    /// Single node of an octree.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    ///
    /// This class is intended for use as an internal data structure by
    /// Octree.
    class OctreeNode<T>
    {

        /// <summary>
        /// Threshold for length comparisons.
        /// </summary>
        private const float Threshold = 1e-4f;

        /// <summary>
        /// Octree to which this node belongs.
        /// </summary>
        private readonly Octree<T> octree;

        /// <summary>
        /// Parent node.
        /// </summary>
        public OctreeNode<T> parentNode;

        /// <summary>
        /// Child nodes indexed by the eight possible combinations of OctreeOctant flags.
        /// </summary>
        public readonly OctreeNode<T>[] childNodes = new OctreeNode<T>[8];

        /// <summary>
        /// Elements contained within the bounds of this node and their positions.
        /// </summary>
        public IDictionary<T, Vector<float>> elementPositions = new Dictionary<T, Vector<float>>();

        /// <summary>
        /// Minimum position (inclusive) of the node's box.
        /// </summary>
        public readonly Vector<float> minPosition;

        /// <summary>
        /// Maximum position (exclusive) of the node's box.
        /// </summary>
        public readonly Vector<float> maxPosition;

        /// <summary>
        /// Partitions between the octants, defined by the planes (xy), (xz), and (yz).
        /// </summary>
        public readonly Vector<float> partitions;

        /// <summary>
        /// Creates a new node of the given octree.
        /// </summary>
        /// <param name="octree">Octree.</param>
        /// <param name="minPosition">Minimum position (inclusive) of the node's box.</param>
        /// <param name="maxPosition">Maximum position (exclusive) of the node's box.</param>
        public OctreeNode(Octree<T> octree, Vector<float> minPosition, Vector<float> maxPosition)
        {
            /* Set fields. */
            this.octree = octree;
            this.minPosition = minPosition;
            this.maxPosition = maxPosition;

            /* Compute node partitions. */
            partitions = Vector.Multiply(0.5f, Vector.Add(minPosition, maxPosition));
        }

        /// <summary>
        /// Gets the octant that contains the given position, assuming the position
        /// is within the bounds.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <returns>Octant containing the position.</returns>
        public OctreeOctant GetOctantForPosition(Vector<float> position)
        {
            return (position[0] >= partitions[0] ? OctreeOctant.East : 0)
                | (position[1] >= partitions[1] ? OctreeOctant.North : 0)
                | (position[2] >= partitions[2] ? OctreeOctant.Top : 0);
        }

        /// <summary>
        /// Gets the bounds of the given octant in this node.
        /// </summary>
        /// <param name="octant">Octant.</param>
        /// <param name="minPosition">Minimum position (inclusive).</param>
        /// <param name="maxPosition">Maximum position (exclusive).</param>
        public void GetBoundsOfOctant(OctreeOctant octant,
            out Vector<float> minPosition, out Vector<float> maxPosition)
        {
            /* Compute minimum position. */
            var minX = octant.HasFlag(OctreeOctant.East) ? partitions[0] : minPosition[0];
            var minY = octant.HasFlag(OctreeOctant.North) ? partitions[1] : minPosition[1];
            var minZ = octant.HasFlag(OctreeOctant.Top) ? partitions[2] : minPosition[2];
            minPosition = new Vector<float>(new float[] { minX, minY, minZ });

            /* Compute maximum position. */
            var maxX = octant.HasFlag(OctreeOctant.East) ? maxPosition[0] : partitions[0];
            var maxY = octant.HasFlag(OctreeOctant.North) ? maxPosition[1] : partitions[1];
            var maxZ = octant.HasFlag(OctreeOctant.Top) ? maxPosition[2] : partitions[2];
            maxPosition = new Vector<float>(new float[] { maxX, maxY, maxZ });
        }

        /// <summary>
        /// Rebalances a leaf node if needed.
        /// </summary>
        /// 
        /// This method assumes that the node is a leaf node.
        private void RebalanceLeafNode()
        {
            /* Depth-first search through the leaf nodes. */
            var nodesToVisit = new Stack<OctreeNode<T>>();
            nodesToVisit.Push(this);
            while (nodesToVisit.Count > 0)
            {
                /* Take the next node off the stack. */
                var currentNode = nodesToVisit.Pop();

                /* If there are less than two elements, the node is already balanced. */
                if (currentNode.elementPositions.Count < 2) continue;

                /* If the node cannot be reduced further in size, stop. */
                if (!CanNodeBeSubdivided()) continue;

                /* If the node cannot be rebalanced further, stop. */
                if (!currentNode.CanLeafNodeBeRebalanced()) continue;

                /* Distribute the elements into the child nodes. */
                DistributeElementsToChildren();
            }
        }

        /// <summary>
        /// Determines whether the node can be further subdivided.
        /// </summary>
        /// <returns></returns>
        private bool CanNodeBeSubdivided()
        {
            var nodeLength = maxPosition[0] - minPosition[0];
            return !(Math.Abs(nodeLength - octree.MinimumNodeSize) < Threshold);
        }

        /// <summary>
        /// Determines whether the node can be rebalanced.
        /// </summary>
        /// <returns>true if the node can be rebalanced, false otherwise.</returns>
        private bool CanLeafNodeBeRebalanced()
        {
            var firstOctant = GetOctantForPosition(elementPositions[elementPositions.Keys.First()]);
            var needToRebalance = false;

            foreach (var element in elementPositions.Keys.Skip(1))
            {
                var nextOctant = GetOctantForPosition(elementPositions[element]);
                needToRebalance = needToRebalance || (nextOctant != firstOctant);
            }

            return needToRebalance;
        }

        /// <summary>
        /// Distributes elements to the child nodes.
        /// </summary>
        private void DistributeElementsToChildren()
        {
            foreach (var element in elementPositions.Keys)
            {
                /* Create the child node if it doesn't already exist. */
                var octant = GetOctantForPosition(elementPositions[element]);
                if (childNodes[(int)octant] == null)
                {
                    GetBoundsOfOctant(octant, out var newMinPos, out var newMaxPos);
                    childNodes[(int)octant] = new OctreeNode<T>(octree, newMinPos, newMaxPos);
                }

                /* Distribute the element. */
                childNodes[(int)octant].elementPositions[element] = elementPositions[element];
            }
        }

    }

}
