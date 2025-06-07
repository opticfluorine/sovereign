// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Sovereign.EngineUtil.Algorithms;

/// <summary>
///     Implements the Union-Find algorithm for a subgraph of the 2D lattice graph.
/// </summary>
public class UnionFind2dGrid
{
    private static readonly (int dx, int dy)[] Neighbors =
    {
        (1, 0), (-1, 0), (0, 1), (0, -1)
    };

    private readonly Dictionary<(int, int), int> componentIds = new();
    private readonly Queue<(int, int)> floodFillQueue = new();
    private readonly Dictionary<(int, int), (int, int)> parent = new();
    private readonly Dictionary<(int, int), int> rank = new();
    private int nextComponentId = 1;

    /// <summary>
    ///     Gets the connected component to which the given vertex belongs, if any.
    /// </summary>
    /// <param name="vertex">Vertex.</param>
    /// <param name="componentId">Component ID. Only valid if method returns true.</param>
    /// <returns>true if the vertex belongs to a connected component, false otherwise.</returns>
    public bool TryGetComponent(ValueTuple<int, int> vertex, out int componentId)
    {
        if (!parent.ContainsKey(vertex))
        {
            componentId = 0;
            return false;
        }

        var root = Find(vertex);
        return componentIds.TryGetValue(root, out componentId);
    }

    /// <summary>
    ///     Adds vertices to the union-find structure.
    /// </summary>
    /// <param name="vertices">Vertices.</param>
    public void AddVertices(IEnumerable<ValueTuple<int, int>> vertices)
    {
        foreach (var v in vertices)
        {
            if (!parent.TryAdd(v, v)) continue;
            componentIds[v] = nextComponentId++;
            rank[v] = 0;

            // Union with neighbors
            foreach (var (dx, dy) in Neighbors)
            {
                var n = (v.Item1 + dx, v.Item2 + dy);
                if (parent.ContainsKey(n)) Union(v, n);
            }
        }
    }

    /// <summary>
    ///     Removes vertices from the union-find structure, relabeling any split components via
    ///     flood fill as necessary.
    /// </summary>
    /// <param name="vertices">Vertices.</param>
    public void RemoveVertices(IEnumerable<ValueTuple<int, int>> vertices)
    {
        var toRemove = new HashSet<(int, int)>(vertices);
        foreach (var v in toRemove)
        {
            parent.Remove(v);
            componentIds.Remove(v);
            rank.Remove(v);
        }

        // Relabel components for affected neighbors
        var visited = new HashSet<(int, int)>();
        foreach (var v in toRemove)
        foreach (var (dx, dy) in Neighbors)
        {
            var n = (v.Item1 + dx, v.Item2 + dy);
            if (parent.ContainsKey(n) && !visited.Contains(n)) FloodFillRelabel(n, nextComponentId++, visited);
        }
    }

    /// <summary>
    ///     Deep copies the union-find structure, returning a new instance with the same state.
    /// </summary>
    /// <returns>New instance with equivalent state.</returns>
    public UnionFind2dGrid Clone()
    {
        var clone = new UnionFind2dGrid();
        foreach (var kv in parent)
            clone.parent[kv.Key] = kv.Value;
        foreach (var kv in componentIds)
            clone.componentIds[kv.Key] = kv.Value;
        foreach (var kv in rank)
            clone.rank[kv.Key] = kv.Value;
        clone.nextComponentId = nextComponentId;
        return clone;
    }

    /// <summary>
    ///     Finds the root of the component containing the given vertex, using path compression.
    /// </summary>
    /// <param name="v">Vertex.</param>
    /// <returns>Root of component.</returns>
    private (int, int) Find((int, int) v)
    {
        if (!parent.ContainsKey(v)) return v;
        if (!parent[v].Equals(v))
            parent[v] = Find(parent[v]);
        return parent[v];
    }

    /// <summary>
    ///     Combines two components into one, relabeling the component IDs as necessary.
    /// </summary>
    /// <param name="a">Vertex in the first component.</param>
    /// <param name="b">Vertex in the second component.</param>
    private void Union((int, int) a, (int, int) b)
    {
        var rootA = Find(a);
        var rootB = Find(b);
        if (rootA.Equals(rootB)) return;

        var idA = componentIds[rootA];
        var idB = componentIds[rootB];
        var minId = Math.Min(idA, idB);

        // Union by rank
        var rankA = rank[rootA];
        var rankB = rank[rootB];
        if (rankA < rankB)
        {
            parent[rootA] = rootB;
            componentIds[rootB] = minId;
            componentIds.Remove(rootA);
        }
        else if (rankA > rankB)
        {
            parent[rootB] = rootA;
            componentIds[rootA] = minId;
            componentIds.Remove(rootB);
        }
        else
        {
            parent[rootB] = rootA;
            componentIds[rootA] = minId;
            componentIds.Remove(rootB);
            rank[rootA]++;
        }
    }

    /// <summary>
    ///     Relabels all reachable vertices by breadth first search.
    /// </summary>
    /// <param name="start">Starting vertex.</param>
    /// <param name="newComponentId">New component ID.</param>
    /// <param name="visited">Set of visited components.</param>
    private void FloodFillRelabel((int, int) start, int newComponentId, HashSet<(int, int)> visited)
    {
        floodFillQueue.Clear();
        floodFillQueue.Enqueue(start);

        var root = Find(start);
        componentIds[root] = newComponentId;

        while (floodFillQueue.TryDequeue(out var v))
        {
            if (!visited.Add(v)) continue;
            parent[v] = v;
            componentIds[v] = newComponentId;
            foreach (var (dx, dy) in Neighbors)
            {
                var n = (v.Item1 + dx, v.Item2 + dy);
                if (parent.ContainsKey(n) && !visited.Contains(n)) floodFillQueue.Enqueue(n);
            }
        }
    }
}