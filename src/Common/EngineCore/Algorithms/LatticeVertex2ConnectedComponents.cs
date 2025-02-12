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

using System.Collections.Generic;

namespace Sovereign.EngineCore.Algorithms;

public struct LatticeVertex2
{
    /// <summary>
    ///     Lattice X coordinate.
    /// </summary>
    public int X;

    /// <summary>
    ///     Lattice Y coordinate.
    /// </summary>
    public int Y;
}

/// <summary>
///     Partitions a partial 2D lattice into its connected components.
/// </summary>
/// <remarks>
///     Two vertices are considered connected by an edge if the distance between them equals 1.
/// </remarks>
public class LatticeVertex2ConnectedComponents
{
    public LatticeVertex2ConnectedComponents(List<LatticeVertex2> vertices)
    {
    }

    /// <summary>
    ///     Single node in the disjoint set.
    /// </summary>
    private class Node
    {
        public Node(LatticeVertex2 vertex)
        {
            Vertex = vertex;
            Parent = this;
            Rank = 0;
        }

        /// <summary>
        ///     Vertex represented by this node of the disjoint set.
        /// </summary>
        public LatticeVertex2 Vertex { get; set; }

        /// <summary>
        ///     Parent node.
        /// </summary>
        public Node Parent { get; set; }

        /// <summary>
        ///     Rank.
        /// </summary>
        public int Rank { get; set; }
    }
}