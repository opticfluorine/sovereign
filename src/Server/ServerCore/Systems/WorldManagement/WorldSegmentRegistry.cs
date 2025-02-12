/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Responsible for tracking which world segments are currently loaded.
/// </summary>
public sealed class WorldSegmentRegistry
{
    /// <summary>
    ///     Set of currently loaded world segment indices.
    /// </summary>
    private readonly ISet<GridPosition> loadedSegments = new HashSet<GridPosition>();

    public bool IsLoaded(GridPosition segmentIndex)
    {
        return loadedSegments.Contains(segmentIndex);
    }

    public void OnSegmentLoaded(GridPosition segmentIndex)
    {
        loadedSegments.Add(segmentIndex);
    }

    public void OnSegmentUnloaded(GridPosition segmentIndex)
    {
        loadedSegments.Remove(segmentIndex);
    }
}