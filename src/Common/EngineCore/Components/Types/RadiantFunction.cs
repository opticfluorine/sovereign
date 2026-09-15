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

using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Scalar function used to evaluate a radiant field contribution.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum RadiantFunction
{
    /// <summary>
    ///     Linear function of distance, evaluated as Param0 * d + Param1 where
    ///     Param0 is the slope, Param1 is the y-intercept, and Param2 is unused.
    /// </summary>
    Linear = 0
}
