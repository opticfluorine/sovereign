/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Sovereign.EngineUtil.Ranges;

namespace Sovereign.EngineUtil.Collections.Octree;

/// <summary>
///     Single node of an octree.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// This class is intended for use as an internal data structure by Octree.
[DebuggerDisplay("{minPosition} - {maxPosition}, Count = {elementPositions.Count}")]
internal sealed class OctreeNode<T> where T : notnull
{
    /// <summary>
    ///     Threshold for length comparisons.
    /// </summary>
    private const float Threshold = 1e-4f;

    /// <summary>
    ///     Child nodes indexed by the eight possible combinations of OctreeOctant flags.
    /// </summary>
    public readonly OctreeNode<T>?[] childNodes = new OctreeNode<T>?[8];

    /// <summary>
    ///     Maximum position (exclusive) of the node's box.
    /// </summary>
    public readonly Vector3 maxPosition;

    /// <summary>
    ///     Minimum position (inclusive) of the node's box.
    /// </summary>
    public readonly Vector3 minPosition;

    private readonly Stack<OctreeNode<T>> nodesToVisit = new(16);

    /// <summary>
    ///     Octree to which this node belongs.
    /// </summary>
    private readonly Octree<T> octree;

    /// <summary>
    ///     Partitions between the octants, defined by the planes (xy), (xz), and (yz).
    /// </summary>
    public readonly Vector3 partitions;

    /// <summary>
    ///     Elements contained within the bounds of this node and their positions.
    /// </summary>
    public IDictionary<T, Vector3> elementPositions = new Dictionary<T, Vector3>();

    /// <summary>
    ///     Parent node.
    /// </summary>
    public OctreeNode<T>? parentNode;

    /// <summary>
    ///     Creates a new node of the given octree.
    /// </summary>
    /// <param name="octree">Octree.</param>
    /// <param name="parentNode">Parent node.</param>
    /// <param name="minPosition">Minimum position (inclusive) of the node's box.</param>
    /// <param name="maxPosition">Maximum position (exclusive) of the node's box.</param>
    public OctreeNode(Octree<T> octree, OctreeNode<T>? parentNode,
        Vector3 minPosition, Vector3 maxPosition)
    {
        /* Set fields. */
        this.octree = octree;
        this.parentNode = parentNode;
        this.minPosition = minPosition;
        this.maxPosition = maxPosition;

        /* Compute node partitions. */
        partitions = 0.5f * (minPosition + maxPosition);
    }

    /// <summary>
    ///     Creates a new parent node for the given child node, growing the tree in the
    ///     direction of the given exterior position.
    /// </summary>
    /// <param name="childNode"></param>
    public OctreeNode(OctreeNode<T> childNode, Vector3 position)
    {
        /* Determine the direction in which the tree should grow. */
        var childOctant = childNode.GetParentNodeDetails(position, out minPosition, out maxPosition);

        /* Arrange the node. */
        childNodes[(int)childOctant] = childNode;
        childNode.parentNode = this;

        /* Copy properties of child node. */
        octree = childNode.octree;
        foreach (var elementPosition in childNode.elementPositions) elementPositions.Add(elementPosition);

        /* Compute node partitions. */
        partitions = 0.5f * (minPosition + maxPosition);
    }

    /// <summary>
    ///     Number of elements at or below this node.
    /// </summary>
    public int Count => elementPositions.Count;

    /// <summary>
    ///     Gets the octant that contains the given position, assuming the position
    ///     is within the bounds.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>Octant containing the position.</returns>
    public OctreeOctant GetOctantForPosition(Vector3 position)
    {
        return (position.X >= partitions.X ? OctreeOctant.East : 0)
               | (position.Y >= partitions.Y ? OctreeOctant.North : 0)
               | (position.Z >= partitions.Z ? OctreeOctant.Top : 0);
    }

    /// <summary>
    ///     Gets the child node that contains the given position, creating the child
    ///     node if necessary.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>Child node.</returns>
    public OctreeNode<T> GetChildNodeForPosition(Vector3 position)
    {
        var octant = GetOctantForPosition(position);

        /* Create the child node if it doesn't exist. */
        if (childNodes[(int)octant] == null)
        {
            GetBoundsOfOctant(octant, out var minPosition, out var maxPosition);
            childNodes[(int)octant] = new OctreeNode<T>(octree, this, minPosition, maxPosition);
        }

        return childNodes[(int)octant]!;
    }

    /// <summary>
    ///     Gets the bounds of the given octant in this node.
    /// </summary>
    /// <param name="octant">Octant.</param>
    /// <param name="octantMinPosition">Minimum position (inclusive).</param>
    /// <param name="octantMaxPosition">Maximum position (exclusive).</param>
    public void GetBoundsOfOctant(OctreeOctant octant,
        out Vector3 octantMinPosition, out Vector3 octantMaxPosition)
    {
        /* Compute minimum position. */
        var minX = octant.HasFlag(OctreeOctant.East) ? partitions.X : minPosition.X;
        var minY = octant.HasFlag(OctreeOctant.North) ? partitions.Y : minPosition.Y;
        var minZ = octant.HasFlag(OctreeOctant.Top) ? partitions.Z : minPosition.Z;
        octantMinPosition = new Vector3(minX, minY, minZ);

        /* Compute maximum position. */
        var maxX = octant.HasFlag(OctreeOctant.East) ? maxPosition.X : partitions.X;
        var maxY = octant.HasFlag(OctreeOctant.North) ? maxPosition.Y : partitions.Y;
        var maxZ = octant.HasFlag(OctreeOctant.Top) ? maxPosition.Z : partitions.Z;
        octantMaxPosition = new Vector3(maxX, maxY, maxZ);
    }

    /// <summary>
    ///     Determines whether the given position is interior to this node.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <returns>true if the position is interior, false otherwise.</returns>
    public bool IsPositionInterior(Vector3 position)
    {
        return RangeUtil.IsPointInRange(minPosition, maxPosition, position);
    }

    /// <summary>
    ///     Determines whether the range of the node intersects the given range.
    /// </summary>
    /// <param name="minRange">Lower bound (inclusive) of the given range.</param>
    /// <param name="maxRange">Upper bound (exclusive) of the given range.</param>
    /// <returns>true if the range intersects, false otherwise.</returns>
    public bool IntersectsRange(Vector3 minRange, Vector3 maxRange)
    {
        return RangeUtil.RangesIntersect(minPosition, maxPosition, minRange, maxRange);
    }

    /// <summary>
    ///     Determines whether the range of the node is completely interior to the given
    ///     range.
    /// </summary>
    /// <param name="minRange">Lower bound (inclusive) of range.</param>
    /// <param name="maxRange">Upper bound (exclusive) of range.</param>
    /// <returns>true if the node is interior, false otherwise.</returns>
    public bool IsNodeInteriorToRange(Vector3 minRange, Vector3 maxRange)
    {
        return RangeUtil.IsRangeInterior(minPosition, maxPosition, minRange, maxRange);
    }

    /// <summary>
    ///     Determines whether this node is a leaf.
    /// </summary>
    /// <returns>true if the node is a leaf, false otherwise.</returns>
    public bool IsLeafNode()
    {
        var isLeaf = true;
        foreach (var child in childNodes) isLeaf = isLeaf && (child == null || child.Count == 0);
        return isLeaf;
    }

    /// <summary>
    ///     Adds the given element to the node, rebalancing if necessary.
    /// </summary>
    /// This method assumes that the node is a leaf node.
    /// <param name="element">Element to be added.</param>
    /// <param name="position">Position of the element.</param>
    public void AddElement(T element, Vector3 position)
    {
        /* Add the element to the node. */
        elementPositions[element] = position;

        /* Propagate upward to the root node. */
        var currentNode = parentNode;
        while (currentNode != null)
        {
            currentNode.elementPositions[element] = position;
            currentNode = currentNode.parentNode;
        }

        /* Rebalance the leaf node. */
        RebalanceLeafNode();
    }

    /// <summary>
    ///     Rebalances a leaf node if needed.
    /// </summary>
    /// This method assumes that the node is a leaf node.
    private void RebalanceLeafNode()
    {
        /* Depth-first search through the leaf nodes. */
        nodesToVisit.Clear();
        nodesToVisit.Push(this);
        while (nodesToVisit.Count > 0)
        {
            /* Take the next node off the stack. */
            var currentNode = nodesToVisit.Pop();

            /* If there are less than two elements, the node is already balanced. */
            if (currentNode.elementPositions.Count < 2) continue;

            /* If the node cannot be reduced further in size, stop. */
            if (!currentNode.CanNodeBeSubdivided()) continue;

            /* If the node cannot be rebalanced further, stop. */
            if (!currentNode.CanLeafNodeBeRebalanced()) continue;

            /* Distribute the elements into the child nodes. */
            currentNode.DistributeElementsToChildren();

            /* Mark child nodes to revisit. */
            foreach (var nextNode in currentNode.childNodes)
                if (nextNode != null)
                    nodesToVisit.Push(nextNode);
        }
    }

    /// <summary>
    ///     Determines the octant in the parent node that this node should be
    ///     assigned to in order to grow the tree toward the given exterior point.
    /// </summary>
    /// <param name="position">Exterior position.</param>
    /// <param name="parentMinPosition">Minimum position of the parent node.</param>
    /// <param name="parentMaxPosition">Maximum position of the parent node.</param>
    /// <returns>Octant containing the current node.</returns>
    private OctreeOctant GetParentNodeDetails(Vector3 position, out Vector3 parentMinPosition,
        out Vector3 parentMaxPosition)
    {
        /* Generate the corners and distances. */
        var corners = new Vector3[8];
        var distancesSquared = new float[8];
        for (var i = 0; i < 8; ++i)
        {
            /* Generate corner. */
            var octant = (OctreeOctant)i;
            corners[i] = new Vector3(
                octant.HasFlag(OctreeOctant.East) ? minPosition.X : maxPosition.X,
                octant.HasFlag(OctreeOctant.North) ? minPosition.Y : maxPosition.Y,
                octant.HasFlag(OctreeOctant.Top) ? minPosition.Z : maxPosition.Z
            );

            /* Compute distance squared. */
            var delta = corners[i] - position;
            distancesSquared[i] = Vector3.Dot(delta, delta);
        }

        /* Select the octant that minimizes the distance. */
        var minDistanceSquared = float.MaxValue;
        var minIndex = 0;
        for (var i = 0; i < 8; ++i)
        {
            var distanceSquared = distancesSquared[i];
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                minIndex = i;
            }
        }

        /* Compute the bounds of the parent node. */
        var childOctant = (OctreeOctant)minIndex;
        var parentHalfWidth = maxPosition.X - minPosition.X;
        parentMinPosition = new Vector3(
            childOctant.HasFlag(OctreeOctant.East) ? minPosition.X - parentHalfWidth : minPosition.X,
            childOctant.HasFlag(OctreeOctant.North) ? minPosition.Y - parentHalfWidth : minPosition.Y,
            childOctant.HasFlag(OctreeOctant.Top) ? minPosition.Z - parentHalfWidth : minPosition.Z
        );
        parentMaxPosition = new Vector3(
            childOctant.HasFlag(OctreeOctant.East) ? maxPosition.X : maxPosition.X + parentHalfWidth,
            childOctant.HasFlag(OctreeOctant.North) ? maxPosition.Y : maxPosition.Y + parentHalfWidth,
            childOctant.HasFlag(OctreeOctant.Top) ? maxPosition.Z : maxPosition.Z + parentHalfWidth
        );

        return childOctant;
    }

    /// <summary>
    ///     Determines whether the node can be further subdivided.
    /// </summary>
    /// <returns></returns>
    private bool CanNodeBeSubdivided()
    {
        var nodeLength = maxPosition.X - minPosition.X;
        return !(Math.Abs(nodeLength - octree.MinimumNodeSize) < Threshold);
    }

    /// <summary>
    ///     Determines whether the node can be rebalanced.
    /// </summary>
    /// <returns>true if the node can be rebalanced, false otherwise.</returns>
    private bool CanLeafNodeBeRebalanced()
    {
        var firstOctant = GetOctantForPosition(elementPositions[elementPositions.Keys.First()]);
        var needToRebalance = false;

        foreach (var element in elementPositions.Keys.Skip(1))
        {
            var nextOctant = GetOctantForPosition(elementPositions[element]);
            needToRebalance = needToRebalance || nextOctant != firstOctant;
        }

        return needToRebalance;
    }

    /// <summary>
    ///     Distributes elements to the child nodes.
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
                childNodes[(int)octant] = new OctreeNode<T>(octree, this, newMinPos, newMaxPos);
            }

            /* Distribute the element. */
            childNodes[(int)octant]!.elementPositions[element] = elementPositions[element];
        }
    }
}