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

using Sovereign.EngineCore.Components.Types;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for retrieving the flags of a single world segment.
/// </summary>
public interface IGetSegmentFlagsQuery
{
    /// <summary>
    ///     Gets the flags for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Flags for the segment, or None if the segment has no stored flags.</returns>
    WorldSegmentFlag GetFlags(GridPosition segmentIndex);
}
