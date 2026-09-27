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
///     Horizontal extent of a stone band within a world generation profile.
/// </summary>
public sealed class StoneBand
{
    /// <summary>
    ///     Lowest Z level covered by the band.
    /// </summary>
    public required int FromZ { get; set; }

    /// <summary>
    ///     Highest Z level covered by the band.
    /// </summary>
    public required int ToZ { get; set; }

    /// <summary>
    ///     Name of the block template entity that fills the band. Resolution to template
    ///     entity IDs happens at plan time.
    /// </summary>
    public required string Template { get; set; }
}
