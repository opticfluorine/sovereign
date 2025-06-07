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

using System.Diagnostics.CodeAnalysis;
using Sovereign.EngineUtil.Algorithms;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Manages the overhead block occupancy graph
/// </summary>
internal class OverheadBlockGraphManager
{
    /// <summary>
    ///     Gets the overhead block graph for the given z if it exists.
    /// </summary>
    /// <param name="z">Z.</param>
    /// <param name="graph">Graph, or null if no graph is found for z.</param>
    /// <returns>true if a graph was found, false otherwise.</returns>
    public bool TryGetGraphForZ(int z, [NotNullWhen(true)] out UnionFind2dGrid? graph)
    {
        graph = null;
        return false;
    }

    /// <summary>
    ///     Readies the overhead block graphs for the next frame.
    /// </summary>
    public void BeginFrame()
    {
        // TODO Flip graph buffers if updated since the last frame.
    }
}