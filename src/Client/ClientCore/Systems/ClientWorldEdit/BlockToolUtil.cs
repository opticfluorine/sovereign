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

using Sovereign.ClientCore.Systems.Camera;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Systems.ClientWorldEdit;

/// <summary>
///     Provides utility methods to support the world editor block tool and related tools.
/// </summary>
public sealed class BlockToolUtil(CameraServices cameraServices, ClientWorldEditState userState)
{
    /// <summary>
    ///     Gets the block coordinate currently hovered by the mouse, taking Z offset into account.
    /// </summary>
    /// <returns>Hovered block coordinate.</returns>
    public GridPosition GetHoveredBlockWithOffset()
    {
        // Select the block whose top face is hovered by the mouse.
        var hoverPos = cameraServices.GetMousePositionWorldCoordinates();
        var posWithOffset = hoverPos with
        {
            Y = hoverPos.Y - userState.ZOffset, Z = hoverPos.Z + userState.ZOffset - 1.0f
        };
        return (GridPosition)posWithOffset;
    }
}