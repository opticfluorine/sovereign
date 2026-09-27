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

namespace Sovereign.WorldGen;

/// <summary>
///     Single cave level within a world generation profile.
/// </summary>
public sealed class CaveLevel
{
    /// <summary>
    ///     Z level of the cave floor.
    /// </summary>
    public required int FloorZ { get; set; }

    /// <summary>
    ///     Vertical clearance of the cave in blocks.
    /// </summary>
    public required int Headroom { get; set; }
}
