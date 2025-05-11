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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Systems.Time;

/// <summary>
///     Enumeration of in-game seasons.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum Season
{
    /// <summary>
    ///     Spring (first quarter of year).
    /// </summary>
    Spring = 0,

    /// <summary>
    ///     Summer (second quarter of year).
    /// </summary>
    Summer = 1,

    /// <summary>
    ///     Fall (third quarter of year).
    /// </summary>
    Fall = 2,

    /// <summary>
    ///     Winter (last quarter of year).
    /// </summary>
    Winter = 3
}