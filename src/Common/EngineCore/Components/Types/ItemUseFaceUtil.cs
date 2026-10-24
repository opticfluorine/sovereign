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

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Shared helpers for item use on block faces.
/// </summary>
public static class ItemUseFaceUtil
{
    /// <summary>
    ///     Gets the cell adjacent to the given block face.
    /// </summary>
    /// <param name="blockPosition">Position of the clicked block.</param>
    /// <param name="face">Clicked block face.</param>
    /// <param name="cell">Adjacent cell. Only meaningful if the method returns true.</param>
    /// <returns>true if the face maps to an adjacent cell, false otherwise.</returns>
    public static bool TryGetAdjacentCell(GridPosition blockPosition, ItemUseFace face, out GridPosition cell)
    {
        // The renderer draws the top face at z + 1 and the south-facing front face at the block's grid
        // position, so the adjacent cells are one step in +Z and one step in -Y respectively.
        switch (face)
        {
            case ItemUseFace.BlockTopFace:
                cell = blockPosition with { Z = blockPosition.Z + 1 };
                return true;

            case ItemUseFace.BlockFrontFace:
                cell = blockPosition with { Y = blockPosition.Y - 1 };
                return true;

            default:
                cell = default;
                return false;
        }
    }
}
