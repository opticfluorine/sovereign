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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     World editor tool handler for block placement and removal.
/// </summary>
public class BlockToolHandler(
    IEventSender eventSender,
    ClientWorldEditInternalController internalController,
    ClientWorldEditState userState,
    BlockToolUtil blockToolUtil) : IWorldEditToolHandler
{
    /// <summary>
    ///     Flag indicating whether the first block has been impacted by the current edit operation.
    /// </summary>
    private bool editStarted;

    /// <summary>
    ///     Block position last impacted by the current edit operation.
    /// </summary>
    private GridPosition lastPosition;

    public void ProcessDraw()
    {
        var hoveredPos = blockToolUtil.GetHoveredBlockWithOffset();
        if (!editStarted || !lastPosition.Equals(hoveredPos))
        {
            editStarted = true;
            lastPosition = hoveredPos;
            ApplyPen(hoveredPos, pos => internalController.SetBlock(eventSender, pos, userState.BlockTemplateId));
        }
    }

    public void ProcessErase()
    {
        var hoveredPos = blockToolUtil.GetHoveredBlockWithOffset();
        if (!editStarted || !lastPosition.Equals(hoveredPos))
        {
            editStarted = true;
            lastPosition = hoveredPos;
            ApplyPen(hoveredPos, pos => internalController.RemoveBlock(eventSender, pos));
        }
    }

    public void Reset()
    {
        editStarted = false;
        lastPosition = default;
    }

    /// <summary>
    ///     Applies the given action to each block under the current pen.
    /// </summary>
    /// <param name="center">Center of pen.</param>
    /// <param name="action">Action to be applied.</param>
    private void ApplyPen(GridPosition center, Action<GridPosition> action)
    {
        var rightStep = userState.PenWidth / 2;
        var leftStep = userState.PenWidth % 2 == 0 ? rightStep - 1 : rightStep;

        for (var x = -leftStep; x <= rightStep; ++x)
        for (var y = -leftStep; y <= rightStep; ++y)
        {
            var pos = center with { X = center.X + x, Y = center.Y + y };
            action(pos);
        }
    }
}