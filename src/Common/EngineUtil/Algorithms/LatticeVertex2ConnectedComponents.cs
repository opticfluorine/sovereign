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
using System.Diagnostics;

namespace Sovereign.EngineUtil.Algorithms;

/// <summary>
///     Partitions a partial 2D lattice into its connected components.
/// </summary>
/// <remarks>
///     Two vertices are considered connected by an edge if the distance between them equals 1.
/// </remarks>
public class LatticeVertex2ConnectedComponents
{
    /// <summary>
    ///     Identifies the connected components in the given 2D lattice graph.
    /// </summary>
    /// <param name="latticeVertices">
    ///     Vertices of a 2D lattice graph where the node (x,y) is represented by the index
    ///     (width * y + x). true indicates a vertex is present, false indicates it is absent.
    /// </param>
    /// <param name="width">Width of the lattice.</param>
    public LatticeVertex2ConnectedComponents(bool[] latticeVertices, int width)
    {
        Debug.Assert(latticeVertices.Length % width == 0);

        var height = latticeVertices.Length / width;
        for (var y = 0; y < height; ++y)
        {
            for (var x = 0; x < width; ++x)
            {
                var index = y * width + x;
                if (latticeVertices[index]) AddConnectedComponent(latticeVertices, x, y, width);
            }
        }
    }

    /// <summary>
    ///     Connected components in the lattice.
    /// </summary>
    public List<List<Tuple<int, int>>> ConnectedComponents { get; } = new();

    /// <summary>
    ///     Greedily adds a connected component from the lattice starting from the given position.
    ///     Consumed vertices are removed from the input matrix.
    /// </summary>
    /// <param name="latticeVertices">Lattice vertices.</param>
    /// <param name="x">Zero-based x coordinate of the starting point.</param>
    /// <param name="y">Zero-based y coordinate of the starting point.</param>
    /// <param name="width">Lattice width.</param>
    private void AddConnectedComponent(bool[] latticeVertices, int x, int y, int width)
    {
        var toVisit = new Stack<Tuple<int, int>>();
        toVisit.Push(Tuple.Create(x, y));

        var component = new List<Tuple<int, int>>();

        while (toVisit.Count > 0)
        {
            var vertex = toVisit.Pop();
            var (localX, localY) = vertex;

            TryVertex(localX - 1, localY, width, latticeVertices, toVisit);
            TryVertex(localX + 1, localY, width, latticeVertices, toVisit);
            TryVertex(localX, localY + 1, width, latticeVertices, toVisit);
            TryVertex(localX, localY - 1, width, latticeVertices, toVisit);

            latticeVertices[localY * width + localX] = false;
        }

        ConnectedComponents.Add(component);
    }

    /// <summary>
    ///     Enqueues the given vertex for a subsequent visit if needed.
    /// </summary>
    /// <param name="x">Lattice x coordinate.</param>
    /// <param name="y">Lattice y coordinate.</param>
    /// <param name="width">Lattice width.</param>
    /// <param name="vertices">Matrix of vertices.</param>
    /// <param name="toVisit">Current stack of vertices to visit.</param>
    private void TryVertex(int x, int y, int width, bool[] vertices, Stack<Tuple<int, int>> toVisit)
    {
        var index = y * width + x;
        if (index < 0 || index >= vertices.Length) return;
        if (vertices[index]) toVisit.Push(Tuple.Create(x, y));
    }
}