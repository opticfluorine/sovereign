using Engine8.EngineUtil.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine8.EngineUtil.Collections
{

    /// <summary>
    /// Generic octree implementation for managing mutable collections of
    /// objects in three-dimensional space.
    /// </summary>
    public class Octree<T>
    {

        /// <summary>
        /// Getter that retrieves the coordinate vector of the given object.
        /// </summary>
        private readonly Func<T, Vector3<float>> pointGetter;

        /// <summary>
        /// Root node of the octree.
        /// </summary>
        private OctreeNode rootNode;

        /// <summary>
        /// Index for O(1) access to the node containing a given object.
        /// </summary>
        private readonly IDictionary<T, OctreeNode> nodeIndex = new Dictionary<T, OctreeNode>();

        /// <summary>
        /// Creates a new octree.
        /// </summary>
        /// <param name="pointGetter">Getter that retrieves the position of each object.</param>
        /// <param name="initialObjects">Objects to construct an octree from.</param>
        public Octree(Func<T, Vector3<float>> pointGetter, IList<T> initialObjects)
        {
            /* Create the initial octree. */
            this.pointGetter = pointGetter;
            rootNode = new OctreeNode(initialObjects, this);
        }

        /// <summary>
        /// Gets the set of objects that have been modified since the
        /// last call to this method.
        /// </summary>
        ///
        /// Note that if there are objects at the exact same position as
        /// a modified object, they may be included in the returned set even
        /// if they are not modified.
        /// 
        /// <returns>Set of modified objects.</returns>
        public ISet<T> GetModifiedObjects()
        {
            /* Depth-first search of the octree. */
            var modifiedObjects = new HashSet<T>();
            var nodeStack = new Stack<OctreeNode>();
            nodeStack.Push(rootNode);

            while (nodeStack.Count > 0)
            {
                /* Get the next node. */
                var nextNode = nodeStack.Pop();

                /* Skip this node if it and its children are unmodified. */
                if (!nextNode.IsModified) continue;
                nextNode.IsModified = false;

                /* If this is a leaf node, take the object; otherwise push children. */
                if (nextNode.IsLeaf)
                    modifiedObjects.UnionWith(nextNode.Objects);
                else
                    nextNode.PushChildNodesOntoStack(nodeStack);
            }

            return modifiedObjects;
        }

        /// <summary>
        /// Enumerates the directions of the eight child ndoes.
        /// </summary>
        /// 
        /// Top/bottom refers to the Z partition (XY plane).
        /// North/south refers to the Y partition (XZ plane).
        /// East/west refers to the X partition (YZ plane).
        /// 
        /// The index of any octant can be obtained by forming the
        /// bitwise-XOR combination of any of these flags. If a flag
        /// is not present, the opposite direction is assumed.
        [Flags]
        private enum Octant
        {
            Top = 1 << 0,
            North = 1 << 1,
            East = 1 << 2,
        }

        /// <summary>
        /// Represents a single octree node.
        /// </summary>
        private class OctreeNode
        {

            /// <summary>
            /// Tolerance for comparing position equality.
            /// </summary>
            private const float BinTolerance = 0.001f;

            /// <summary>
            /// Parent node, or null if this is the root node.
            /// </summary>
            public OctreeNode ParentNode { get; private set; }

            /// <summary>
            /// Child nodes indexed by OctreeNodeDirection.
            /// </summary>
            public OctreeNode[] ChildNodes { get; private set; } = new OctreeNode[8];

            /// <summary>
            /// Minimum extents of the node.
            /// </summary>
            public Vector3<float> MinExtents { get; private set; }

            /// <summary>
            /// Maximum extents of the node.
            /// </summary>
            public Vector3<float> MaxExtents { get; private set; }

            /// <summary>
            /// Partitions between octants.
            /// </summary>
            public Vector3<float> Partitions { get; private set; }

            /// <summary>
            /// Whether this node or any child has been modified since the last call
            /// to GetModifiedObjects().
            /// </summary>
            public bool IsModified;

            /// <summary>
            /// Whether this node is a leaf node.
            /// </summary>
            public bool IsLeaf { get; private set; }

            /// <summary>
            /// Objects at or below this node.
            /// </summary>
            public readonly ISet<T> Objects = new HashSet<T>();

            /// <summary>
            /// Octree.
            /// </summary>
            private Octree<T> octree;

            /// <summary>
            /// Creates an initial root node from the given objects.
            /// </summary>
            /// <param name="objects">Objects to be added.</param>
            /// <param name="octree">Octree that this node belongs to.</param>
            public OctreeNode(IEnumerable<T> objects, Octree<T> octree)
            {
                this.octree = octree;

                /* The initial extents are unknown, measure them. */
                GetExtents(objects, out var minExtents, out var maxExtents);

                /* Now create the node. */
                CreateNode(objects, minExtents, maxExtents);
            }

            /// <summary>
            /// Creates a child node containing the given objects.
            /// </summary>
            /// <param name="objects">Objects to be added.</param>
            /// <param name="octree">Octree that this node belongs to.</param>
            /// <param name="minExtents">Minimum extent in each coordinate.</param>
            /// <param name="maxExtents">Maximum extent in each coordinate.</param>
            public OctreeNode(IEnumerable<T> objects, Octree<T> octree, Vector3<float> minExtents,
                Vector3<float> maxExtents, OctreeNode parent)
            {
                this.octree = octree;
                ParentNode = parent;

                /* Extents are known, so create the node. */
                CreateNode(objects, minExtents, maxExtents);
            }

            /// <summary>
            /// Pushes all of the child nodes onto the stack.
            /// </summary>
            /// <param name="stack"></param>
            public void PushChildNodesOntoStack(Stack<OctreeNode> stack)
            {
                for (int i = 0; i < 8; ++i)
                    stack.Push(ChildNodes[i]);
            }

            /// <summary>
            /// Gets the child node that contains the given point.
            /// </summary>
            /// <param name="point">Point.</param>
            /// <returns>Child node that contains the given point.</returns>
            public OctreeNode GetNodeForPoint(Vector3<float> point)
            {
                return ChildNodes[(int)GetOctantForPoint(point)];
            }

            /// <summary>
            /// Gets the index of the octant that contains the given point.
            /// </summary>
            /// <param name="point">Point.</param>
            /// <returns>Index of the octant that contains the point.</returns>
            private Octant GetOctantForPoint(Vector3<float> point)
            {
                var isTop = point.z > Partitions.z;
                var isNorth = point.y > Partitions.y;
                var isEast = point.x > Partitions.x;

                return (isTop ? Octant.Top : 0)
                    | (isNorth ? Octant.North : 0)
                    | (isEast ? Octant.East : 0);
            }

            /// <summary>
            /// Computes the minimum extents along each coordinate for the given
            /// collection of objects.
            /// </summary>
            /// <param name="objects">Objects.</param>
            /// <param name="minExtents">Minimum extents.</param>
            /// <param name="maxExtents">Maximum extents.</param>
            private void GetExtents(IEnumerable<T> objects, out Vector3<float> minExtents,
                out Vector3<float> maxExtents)
            {
                /* Compute extents. */
                var points = objects.Select(obj => octree.pointGetter(obj));
                var x = points.Select(point => point.x);
                var y = points.Select(point => point.y);
                var z = points.Select(point => point.z);

                minExtents = new Vector3<float>()
                {
                    x = x.Min(),
                    y = y.Min(),
                    z = z.Min()
                };

                maxExtents = new Vector3<float>()
                {
                    x = x.Max(),
                    y = y.Max(),
                    z = z.Max()
                };
            }

            /// <summary>
            /// Creates the node.
            /// </summary>
            /// <param name="objects">Objects to be added.</param>
            /// <param name="minExtents">Minimum extents.</param>
            /// <param name="maxExtents">Maximum extents.</param>
            private void CreateNode(IEnumerable<T> objects, Vector3<float> minExtents,
                Vector3<float> maxExtents)
            {
                /* Start this node as modified. */
                IsModified = true;

                /* Bisect the axes to get the new partitions. */
                MinExtents = minExtents;
                MaxExtents = maxExtents;
                Partitions = new Vector3<float>()
                {
                    x = (MinExtents.x + MaxExtents.x) * 0.5f,
                    y = (MinExtents.y + MaxExtents.y) * 0.5f,
                    z = (MinExtents.z + MaxExtents.z) * 0.5f
                };

                /* Determine whether this node is a leaf. */
                Objects.UnionWith(objects);
                IsLeaf = CheckIsLeaf(objects);
                if (IsLeaf)
                {
                    /* Index the objects. */
                    foreach (var obj in objects)
                    {
                        octree.nodeIndex[obj] = this;
                    }
                }

                /* If this isn't a leaf, partition the objects. */
                Objects.UnionWith(objects);
                var octantSets = PartitionObjects(objects);

                /* Create the child nodes. */
                CreateChildNodes(octantSets);
            }

            /// <summary>
            /// Creates the child nodes.
            /// </summary>
            /// <param name="octantSets">Partitioned object sets.</param>
            private void CreateChildNodes(ISet<T>[] octantSets)
            {
                /* Iterate over the octants. */
                for (int i = 0; i < 8; ++i)
                {
                    /* Skip if the octant is empty. */
                    var octantSet = octantSets[i];
                    if (octantSet.Count == 0) continue;

                    /* Compute the boundaries of the octant. */
                    var octant = (Octant)i;
                    var newMinExtents = new Vector3<float>()
                    {
                        x = octant.HasFlag(Octant.East) ? Partitions.x : MinExtents.x,
                        y = octant.HasFlag(Octant.North) ? Partitions.y : MinExtents.y,
                        z = octant.HasFlag(Octant.Top) ? Partitions.z : MinExtents.z
                    };
                    var newMaxExtents = new Vector3<float>()
                    {
                        x = octant.HasFlag(Octant.East) ? MaxExtents.x : Partitions.x,
                        y = octant.HasFlag(Octant.North) ? MaxExtents.y : Partitions.y,
                        z = octant.HasFlag(Octant.Top) ? MaxExtents.z : Partitions.z 
                    };

                    /* Create the child node. */
                    ChildNodes[i] = new OctreeNode(octantSet, octree,
                        newMinExtents, newMaxExtents, this);
                }
            }

            /// <summary>
            /// Partitions a set of objects into the octants of this node.
            /// </summary>
            /// <param name="objects">Objects to be partitioned.</param>
            /// <returns>Partitioned set of objects.</returns>
            private ISet<T>[] PartitionObjects(IEnumerable<T> objects)
            {
                /* Allocate the empty partition sets. */
                var octantSets = new ISet<T>[8];
                for (int i = 0; i < 8; ++i)
                    octantSets[i] = new HashSet<T>();
                
                foreach (T obj in objects)
                {
                    /* Determine which octant contains this object. */
                    var point = octree.pointGetter(obj);
                    var octant = GetOctantForPoint(point);

                    /* Partition the object. */
                    octantSets[(int)octant].Add(obj);
                }

                return octantSets;
            }

            /// <summary>
            /// Determines whether this node is a leaf.
            /// </summary>
            /// <param name="objects"></param>
            /// <returns>true if the node is a leaf, false otherwise.</returns>
            private bool CheckIsLeaf(IEnumerable<T> objects)
            {
                /* Not a leaf if any two objects are separated in space by more than the tolerance. */
                var obj0 = objects.First();
                var point0 = octree.pointGetter(obj0);
                foreach (var obj in objects.Skip(1))
                {
                    /* Check if the points are separated. */
                    var point = octree.pointGetter(obj);
                    var dx = Math.Abs(point.x - point0.x);
                    var dy = Math.Abs(point.y - point0.y);
                    var dz = Math.Abs(point.z - point0.z);
                    if (dx < BinTolerance && dy < BinTolerance && dz < BinTolerance)
                        return false;
                }

                /* If we get here, all points are within the tolerance. */
                return true;
            }

        }

    }

}
