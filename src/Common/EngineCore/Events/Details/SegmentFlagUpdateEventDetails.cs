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

namespace Sovereign.EngineCore.Events.Details;

/// <summary>
///     Details for a request to set or clear a world segment flag over an inclusive box of
///     world segments.
/// </summary>
public sealed class SegmentFlagUpdateEventDetails : IEventDetails
{
    /// <summary>
    ///     Minimum (inclusive) corner of the affected segment box.
    /// </summary>
    public GridPosition MinSegment;

    /// <summary>
    ///     Maximum (inclusive) corner of the affected segment box.
    /// </summary>
    public GridPosition MaxSegment;

    /// <summary>
    ///     Flag to apply.
    /// </summary>
    public WorldSegmentFlag Flag;

    /// <summary>
    ///     Whether to set the flag (true) or clear it (false).
    /// </summary>
    public bool Set;
}
