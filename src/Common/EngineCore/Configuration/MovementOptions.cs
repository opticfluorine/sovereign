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

namespace Sovereign.EngineCore.Configuration;

/// <summary>
///     User-configurable options for movement.
/// </summary>
public sealed class MovementOptions
{
    /// <summary>
    ///     Interval between movement requests in ticks.
    /// </summary>
    public int RequestIntervalTicks { get; set; } = 15;

    /// <summary>
    ///     Number of ticks until a move request expires.
    /// </summary>
    public int MoveExpirationTicks { get; set; } = 20;
}