// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System.Numerics;

namespace Sovereign.ClientCore.Systems.Perspective;

/// <summary>
///     Public read API for perspective information.
/// </summary>
public class PerspectiveServices
{
    private readonly PerspectiveLineManager lineManager;

    public PerspectiveServices(PerspectiveLineManager lineManager)
    {
        this.lineManager = lineManager;
    }

    /// <summary>
    ///     Gets the highest entity appearing to overlap a position within a given z window.
    /// </summary>
    /// <param name="position">Position in world coordinates to overlap.</param>
    /// <param name="minimumZ">Minimum Z for window.</param>
    /// <param name="maximumZ"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool TryGetHighestCoveringEntity(Vector3 position, float minimumZ, float maximumZ, out ulong entityId)
    {
        return lineManager.TryGetHighestEntityAtPoint(position, minimumZ, maximumZ, out entityId);
    }
}