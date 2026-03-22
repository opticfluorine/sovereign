// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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
using System.Threading;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages highlighting of entities.
/// </summary>
public sealed class HighlightManager
{
    private readonly Lock blockLock = new();
    private readonly HashSet<GridPosition> highlightSet = new();

    /// <summary>
    ///     Block highlight state.
    /// </summary>
    public Highlight BlockHighlightState
    {
        get
        {
            lock (blockLock)
            {
                return field;
            }
        }
        set
        {
            lock (blockLock)
            {
                field = value;
            }
        }
    }

    /// <summary>
    ///     Copies the block highlight set into a destination buffer. The buffer is not cleared before copy.
    /// </summary>
    /// <param name="destination">Destination buffer.</param>
    public void GetBlockHighlights(HashSet<GridPosition> destination)
    {
        lock (blockLock)
        {
            destination.UnionWith(highlightSet);
        }
    }

    /// <summary>
    ///     Clears any current block highlights.
    /// </summary>
    public void ClearBlockHighlight()
    {
        lock (blockLock)
        {
            highlightSet.Clear();
        }
    }

    /// <summary>
    ///     Adds a block highlight at the given block position.
    /// </summary>
    /// <param name="position">Block position to highlight.</param>
    public void AddBlockHighlight(GridPosition position)
    {
        lock (blockLock)
        {
            highlightSet.Add(position);
        }
    }

    /// <summary>
    ///     Adds a rectangle in the XY plane to the block highlights.
    /// </summary>
    /// <param name="bottomLeft">Bottom left block position.</param>
    /// <param name="width">Width in blocks along X axis.</param>
    /// <param name="height">Height in blocks along Y axis.</param>
    public void AddBlockHighlightXyRect(GridPosition bottomLeft, uint width, uint height)
    {
        lock (blockLock)
        {
            for (var x = bottomLeft.X; x < bottomLeft.X + width; ++x)
            {
                for (var y = bottomLeft.Y; y < bottomLeft.Y + height; ++y)
                {
                    highlightSet.Add(new GridPosition(x, y, bottomLeft.Z));
                }
            }
        }
    }

    /// <summary>
    ///     Adds a square block highlight centered on the given block.
    /// </summary>
    /// <param name="center">Center block position.</param>
    /// <param name="width">Width of the square.</param>
    public void AddBlockHighlightSquare(GridPosition center, uint width)
    {
        var rightStep = width / 2;
        var leftStep = width % 2 == 0 ? rightStep - 1 : rightStep;
        var bottomLeft = new GridPosition((int)(center.X - leftStep), (int)(center.Y - leftStep), center.Z);

        AddBlockHighlightXyRect(bottomLeft, width, width);
    }
}