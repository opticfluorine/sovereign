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

using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.ServerCore.Systems.WorldManagement;

/// <summary>
///     Interface for unloading world segments.
/// </summary>
public interface IWorldSegmentUnloader
{
    /// <summary>
    ///     Unloads the given world segment.
    /// </summary>
    /// <param name="segmentIndex">Index to world segment.</param>
    void UnloadSegment(GridPosition segmentIndex);
}