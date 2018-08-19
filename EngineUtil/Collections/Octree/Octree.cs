﻿using Engine8.EngineUtil.Ranges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Engine8.EngineUtil.Collections.Octree
{

    /// <summary>
    /// Octree implementation for managing mutable sets of
    /// distinct objects in three-dimensional space.
    /// </summary>
    ///
    /// The Octree class is thread-safe.
    public sealed class Octree<T>
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
        public static readonly Vector3 DefaultOrigin = Vector3.Zero;

        /// <summary>
        /// Minimum length of a node.
        /// </summary>
        public readonly float MinimumNodeSize;

        /// <summary>
        /// The number of elements known to the octree.
        /// </summary>
        public int Count => rootNode.elementPositions.Count;

        /// <summary>
        /// Root node of the octree.
        /// </summary>
        private OctreeNode<T> rootNode;

        /// <summary>
        /// Object used for locking.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Creates an empty octree.
        /// </summary>
        /// <param name="origin">Center point of the initial tree.</param>
        /// <param name="initialLevels">Number of levels to initially generate in the tree.</param>
        /// <param name="minimumNodeSize">Minimum length of a node.</param>
        public Octree(Vector3 origin,
            int initialLevels = DefaultInitialLevels, 
            float minimumNodeSize = DefaultMinimumNodeSize)
        {
            MinimumNodeSize = minimumNodeSize;

            /* Create the initial node. */
            var halfRootNodeLength = 0.5f * (float)Math.Pow(minimumNodeSize, initialLevels);
            var span = new Vector3(halfRootNodeLength);
            var minPosition = origin - span;
            var maxPosition = origin + span;
             
            rootNode = new OctreeNode<T>(this, null, minPosition, maxPosition);
        }

        /// <summary>
        /// Creates an octree containing the given objects.
        /// </summary>
        /// <param name="origin">Center point of the initial tree.</param>
        /// <param name="initialData">Initial elements to be added to the octree.</param>
        /// <param name="initialLevels">Number of levels to initially generate in the tree.</param>
        /// <param name="minimumNodeSize">Minimum length of a node.</param>
        public Octree(Vector3 origin,
            IDictionary<T, Vector3> initialData,
            int initialLevels = DefaultInitialLevels,
            float minimumNodeSize = DefaultMinimumNodeSize) : this(origin, initialLevels, minimumNodeSize)
        {
            /* Shouldn't need to lock yet, but needed to assure Add() below. */
            using (var octreeLock = AcquireLock())
            {
                /* Add all initial points to the octree. */
                foreach (var element in initialData.Keys)
                {
                    var position = initialData[element];
                    Add(octreeLock, position, element);
                }
            }
        }
        
        /// <summary>
        /// Adds the given element to the octree at the given position.
        /// </summary>
        /// 
        /// If the element is already in the octree, this method has no effect.
        /// 
        /// <param name="lockHandle">Active lock handle.</param>
        /// <param name="position">Initial position of the element.</param>
        /// <param name="element">Element to be added.</param>
        public void Add(OctreeLock lockHandle, Vector3 position, T element)
        {
            /* Find or create the leaf node that will contain the element. */
            var leafNode = FindLeafNodeForPosition(position);

            /* Add the element to the leaf node. */
            leafNode.AddElement(element, position);
        }

        /// <summary>
        /// Updates the position of the given element.
        /// </summary>
        /// 
        /// <param name="lockHandle">Active lock handle.</param>
        /// <param name="element">Element to be updated.</param>
        /// <param name="position">New position of the element.</param>
        ///
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the element is not in the octree.
        /// </exception>
        public void UpdatePosition(OctreeLock lockHandle, T element, Vector3 position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the given element from the octree.
        /// </summary>
        /// 
        /// <param name="lockHandle">Active lock handle.</param>
        /// <param name="element">Element to be removed.</param>
        public void Remove(OctreeLock lockHandle, T element)
        {
            /* Scan down from the root node. */
            var currentNode = rootNode;
            while (currentNode != null)
            {
                /* Remove item from the current node. */
                currentNode.elementPositions.Remove(element);

                /* Descend if needed. */
                OctreeNode<T> nextNode = null;
                foreach (var childNode in currentNode.childNodes)
                {
                    if (childNode == null) continue;
                    if (childNode.elementPositions.ContainsKey(element))
                        nextNode = childNode;
                }
                currentNode = nextNode;
            }
        }

        /// <summary>
        /// Gets the elements in the given range.
        /// </summary>
        /// <param name="lockHandle">Active lock handle.</param>
        /// <param name="minPosition">Minimum position (inclusive).</param>
        /// <param name="maxPosition">Maximum position (exclusive).</param>
        /// <param name="buffer">Buffer to hold the results.</param>
        public void GetElementsInRange(OctreeLock lockHandle, Vector3 minPosition,
            Vector3 maxPosition, IList<Tuple<Vector3, T>> buffer)
        {
            /* Depth-first search of overlapping nodes. */
            var nodesToSearch = new Stack<OctreeNode<T>>();
            nodesToSearch.Push(rootNode);
            while (nodesToSearch.Count > 0)
            {
                var currentNode = nodesToSearch.Pop();

                /* Only consider this node if the search range intersects. */
                if (!currentNode.IntersectsRange(minPosition, maxPosition)) continue;

                /* If the current node is entirely inside the range, take all objects. */
                if (currentNode.IsNodeInteriorToRange(minPosition, maxPosition))
                {
                    foreach (var elementAndPosition in currentNode.elementPositions)
                    {
                        var element = elementAndPosition.Key;
                        var position = elementAndPosition.Value;
                        buffer.Add(new Tuple<Vector3, T>(position, element));
                    }
                    continue;
                }

                /* If not a leaf node, descend to the child nodes. */
                if (!currentNode.IsLeafNode())
                {
                    foreach (var childNode in currentNode.childNodes)
                    {
                        if (childNode != null) nodesToSearch.Push(childNode);
                    }
                    continue;
                }

                /* Otherwise take only the objects that are interior. */
                foreach (var elementAndPosition in currentNode.elementPositions)
                {
                    var position = elementAndPosition.Value;
                    if (RangeUtil.IsPointInRange(minPosition, maxPosition, position))
                    {
                        var element = elementAndPosition.Key;
                        buffer.Add(new Tuple<Vector3, T>(position, element));
                    }
                }
            }
        }

        /// <summary>
        /// Acquires a lock for performing octree operations. This operation blocks
        /// until a lock is acquired.
        /// </summary>
        /// <returns>Active octree lock.</returns>
        public OctreeLock AcquireLock()
        {
            return new OctreeLock(this);
        }

        /// <summary>
        /// Attempts to acquire a lock for performing octree operations without blocking.
        /// </summary>
        /// <param name="octreeLock">Octree lock. Undefined if this method returns false.</param>
        /// <returns>true if a lock was acquired, false otherwise.</returns>
        public bool TryAcquireLock(out OctreeLock octreeLock)
        {
            var acquired = Monitor.TryEnter(lockObject);
            octreeLock = acquired ? AcquireLock() : null;
            return acquired;
        }

        /// <summary>
        /// Finds the leaf node that contains the given position. The leaf node
        /// is created if it does not already exist.
        /// </summary>
        /// <param name="position">Position for which the leaf node is to be found.</param>
        /// <returns>Leaf node.</returns>
        private OctreeNode<T> FindLeafNodeForPosition(Vector3 position)
        {
            /* Expand the tree if needed. */
            while (!rootNode.IsPositionInterior(position))
                rootNode = new OctreeNode<T>(rootNode, position);

            /* Find/create the leaf node. */
            return DescendToLeafNode(position);
        }

        /// <summary>
        /// Descends to the leaf node for the given position, creating the leaf node
        /// if necessary.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <returns>Leaf node.</returns>
        private OctreeNode<T> DescendToLeafNode(Vector3 position)
        {
            /* Drill down to the leaf node. */
            var currentNode = rootNode;
            while (!currentNode.IsLeafNode())
            {
                /* Descend further. */
                currentNode = currentNode.GetChildNodeForPosition(position);
            }

            return currentNode;
        }

        /// <summary>
        /// Handle class for locking the octree.
        /// </summary>
        public sealed class OctreeLock : IDisposable
        {

            private readonly Octree<T> octree;

            internal OctreeLock(Octree<T> octree)
            {
                this.octree = octree;
                Monitor.Enter(octree.lockObject);
            }

            public void Dispose()
            {
                Monitor.Exit(octree.lockObject);
            }

        }

    }

}