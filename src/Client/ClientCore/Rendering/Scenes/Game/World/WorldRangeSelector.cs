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
using System.Numerics;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ClientCore.Rendering.Scenes.Game.World;

/// <summary>
///     Utility class for determining ranges used in world rendering.
/// </summary>
public class WorldRangeSelector
{
    private readonly ClientConfigurationManager configManager;
    private readonly DisplayViewport viewport;

    public WorldRangeSelector(DisplayViewport viewport, ClientConfigurationManager configManager)
    {
        this.viewport = viewport;
        this.configManager = configManager;
    }

    /// <summary>
    ///     Determines the search extents as a 2D grid of block positions. The perspective lines
    ///     which intersect this grid will be searched for drawable entities.
    /// </summary>
    /// <param name="minExtent">Minimum extent.</param>
    /// <param name="maxExtent">Maximum extent.</param>
    /// <param name="centerPos">Center position in world coordinates.</param>
    public void DetermineExtents(out GridPosition minExtent, out GridPosition maxExtent,
        Vector3 centerPos)
    {
        var halfX = viewport.WidthInTiles * 0.5f;
        var halfY = viewport.HeightInTiles * 0.5f;

        var clientConfiguration = configManager.ClientConfiguration;
        var minX = (int)Math.Floor(centerPos.X - halfX - clientConfiguration.RenderSearchSpacerX);
        var maxX = (int)Math.Floor(centerPos.X + halfX + clientConfiguration.RenderSearchSpacerX);

        var minY = (int)Math.Ceiling(centerPos.Y - halfY - clientConfiguration.RenderSearchSpacerY);
        var maxY = (int)Math.Ceiling(centerPos.Y + halfY + clientConfiguration.RenderSearchSpacerY);

        var z = (int)Math.Floor(centerPos.Z);

        minExtent = new GridPosition(minX, minY, z);
        maxExtent = new GridPosition(maxX, maxY, z);
    }
}