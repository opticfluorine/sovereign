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
using System.Numerics;
using Sovereign.EngineUtil.Collections.Octree;
using Xunit;

namespace TestEngineUtil.Collections.Octree;

/// <summary>
///     Unit tests for the Octree class.
/// </summary>
public class TestOctree
{
    /// <summary>
    ///     Tests that an empty octree can be created.
    /// </summary>
    [Fact]
    public void TestCreateEmptyOctree()
    {
        /* Create empty octree. */
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin);

        /* Assert that the resulting octree is empty. */
        Assert.Equal(0, octree.Count);
    }

    /// <summary>
    ///     Tests that an octree can be created with initial data.
    /// </summary>
    [Fact]
    public void TestCreateNonEmptyOctree()
    {
        /* Create an octree with a single item. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(1.0f, 0.0f, 0.0f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Assert that the resulting octree contains one item. */
        Assert.Equal(1, octree.Count);
    }

    /// <summary>
    ///     Tests that a range query that does not match any elements returns
    ///     the empty set.
    /// </summary>
    [Fact]
    public void TestEmptyRangeQuery()
    {
        /* Create an octree with a single item. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(1.0f, 0.0f, 0.0f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Query a range that matches nothing. */
        var minRange = new Vector3(1.2f, 0.0f, 0.0f);
        var maxRange = new Vector3(2.4f, 1.0f, 1.0f);
        var matchList = new List<Tuple<Vector3, ulong>>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, minRange, maxRange, matchList,
                (pos, id) => new Tuple<Vector3, ulong>(pos, id));
        }

        /* Assert that nothing matched. */
        Assert.Empty(matchList);
    }

    /// <summary>
    ///     Tests that a range query that matches elements returns the correct elements.
    /// </summary>
    [Fact]
    public void TestRangeQuery()
    {
        /* Create an octree with a single item. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(1.0f, 0.0f, 0.0f),
            [1] = new(1.6f, 0.2f, 0.8f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Query a range that matches nothing. */
        var minRange = new Vector3(1.2f, 0.0f, 0.0f);
        var maxRange = new Vector3(2.4f, 1.0f, 1.0f);
        var matchList = new List<Tuple<Vector3, ulong>>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, minRange, maxRange, matchList,
                (pos, id) => new Tuple<Vector3, ulong>(pos, id));
        }

        /* Assert that the correct element matched. */
        var matchingTuple = new Tuple<Vector3, ulong>(data[1], 1);
        Assert.Contains(matchingTuple, matchList);
    }

    /// <summary>
    ///     Tests that an element can be added to the octree.
    /// </summary>
    [Theory]
    [InlineData(0.1f)] /* Requires subdividing the leaf node. */
    [InlineData(2048.0f)] /* Requires expanding the tree. */
    public void TestQueryAfterAddElement(float secondElementScaleFactor)
    {
        /* Create a single-element octree. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(4.0f, 4.0f, 4.0f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Add the second element. */
        var secondPosition = secondElementScaleFactor * data[0];
        using (var octreeLock = octree.AcquireLock())
        {
            octree.Add(octreeLock, secondPosition, 1);
        }

        /* Assert that the octree contains two elements. */
        Assert.Equal(2, octree.Count);

        /* Assert that range queries still work correctly. */
        var origin = new Vector3(0.0f, 0.0f, 0.0f);
        var midpoint = 0.5f * (data[0] + secondPosition);
        var matchList = new List<Tuple<Vector3, ulong>>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, origin, midpoint, matchList,
                (pos, id) => new Tuple<Vector3, ulong>(pos, id));
        }

        Assert.Single(matchList);
    }

    [Fact]
    public void TestQueryAfterUpdateElement()
    {
        /* Create an octree with three elements in separate bins. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(1.1f, 1.1f, 1.1f),
            [1] = new(2.1f, 2.1f, 2.1f),
            [2] = new(3.1f, 3.1f, 3.1f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Move the second element into the same bin as the first. */
        var newSecondPos = new Vector3(1.2f, 1.2f, 1.2f);
        using (var octreeLock = octree.AcquireLock())
        {
            octree.UpdatePosition(octreeLock, 1, newSecondPos);
        }

        /* Assert that a range query on the first bin will find the first two values. */
        var minRange = new Vector3(1.0f, 1.0f, 1.0f);
        var maxRange = new Vector3(1.3f, 1.3f, 1.3f);
        var matchList = new List<Tuple<Vector3, ulong>>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, minRange, maxRange, matchList,
                (pos, id) => new Tuple<Vector3, ulong>(pos, id));
        }

        var firstTuple = new Tuple<Vector3, ulong>(data[0], 0);
        var secondTuple = new Tuple<Vector3, ulong>(newSecondPos, 1);

        Assert.Equal(2, matchList.Count);
        Assert.Contains(firstTuple, matchList);
        Assert.Contains(secondTuple, matchList);
    }

    [Fact]
    public void TestRemoveElement()
    {
        /* Create an octree with two elements. */
        var data = new Dictionary<ulong, Vector3>
        {
            [0] = new(1.0f, 1.0f, 1.0f),
            [1] = new(1.0f, 1.0f, 1.0f)
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        /* Remove the first element from the octree. */
        using (var octreeLock = octree.AcquireLock())
        {
            octree.Remove(octreeLock, 0);
        }

        /* Assert that the second element is still present. */
        Assert.Equal(1, octree.Count);
        var remainingTuple = new Tuple<Vector3, ulong>(data[1], 1);
        var minRange = new Vector3(1.0f, 1.0f, 1.0f);
        var maxRange = new Vector3(1.2f, 1.2f, 1.2f);
        var matchList = new List<Tuple<Vector3, ulong>>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, minRange, maxRange, matchList,
                (pos, id) => new Tuple<Vector3, ulong>(pos, id));
        }

        Assert.Single(matchList);
        Assert.Contains(remainingTuple, matchList);
    }

    [Theory]
    [InlineData(true, 0.5f, 0.0f, 0.0f)]
    [InlineData(true, 0.0f, 0.5f, 0.0f)]
    [InlineData(true, 0.0f, 0.0f, 0.5f)]
    [InlineData(false, 0.5f, 0.0f, 0.0f)]
    [InlineData(false, 0.0f, 0.5f, 0.0f)]
    [InlineData(false, 0.0f, 0.0f, 0.5f)]
    public void TestOutwardExpansion(bool fromMaxExtent, float deltaX, float deltaY, float deltaZ)
    {
        // Create octree with a single element.
        const ulong firstId = 0;
        const ulong secondId = 1;
        var firstPoint = new Vector3(1.0f);
        var data = new Dictionary<ulong, Vector3>
        {
            [firstId] = firstPoint
        };
        var octree = new Octree<ulong>(Octree<ulong>.DefaultOrigin, data);

        // Force the octree to expand.
        var delta = new Vector3(deltaX, deltaY, deltaZ);
        var secondPoint = fromMaxExtent ? octree.MaximumExtent + delta : octree.MinimumExtent - delta;
        using (var octreeLock = octree.AcquireLock())
        {
            octree.Add(octreeLock, secondPoint, secondId);
        }

        // Check total number of elements.
        Assert.Equal(2, octree.Count);

        // Query the full extent, expect to get both positions.
        var queryResults = new List<ulong>();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, octree.MinimumExtent, octree.MaximumExtent, queryResults,
                (_, id) => id);
        }

        Assert.Contains(firstId, queryResults);
        Assert.Contains(secondId, queryResults);

        // Query a subrange just excluding the second element.
        queryResults.Clear();
        using (var octreeLock = octree.AcquireLock())
        {
            octree.GetElementsInRange(octreeLock, fromMaxExtent ? firstPoint : 0.99f * secondPoint,
                fromMaxExtent ? 0.99f * secondPoint : 1.01f * firstPoint, queryResults,
                (_, id) => id);
        }

        Assert.Contains(firstId, queryResults);
        Assert.DoesNotContain(secondId, queryResults);
    }
}