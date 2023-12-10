/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
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

using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Handles server-side postprocessing of newly loaded world segments.
/// </summary>
public class ServerWorldSegmentLoadedHandler : IWorldSegmentLoadedHandler
{
    private readonly WorldSegmentBlockDataManager blockDataManager;

    public ServerWorldSegmentLoadedHandler(WorldSegmentBlockDataManager blockDataManager)
    {
        this.blockDataManager = blockDataManager;
    }

    public void OnWorldSegmentLoaded(GridPosition segmentIndex)
    {
        blockDataManager.AddWorldSegment(segmentIndex);
    }
}