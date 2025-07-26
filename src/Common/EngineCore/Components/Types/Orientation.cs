// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using System;
using System.Numerics;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Specifies a directional orientation.
/// </summary>
[Scriptable]
[ScriptableEnum]
public enum Orientation
{
    South = 0,
    Southeast = 1,
    East = 2,
    Northeast = 3,
    North = 4,
    Northwest = 5,
    West = 6,
    Southwest = 7
}

/// <summary>
///     Helper functions for working with orientations.
/// </summary>
public static class OrientationUtil
{
    /// <summary>
    ///     Gets a unit vector pointing in the direction of the given orientation.
    /// </summary>
    /// <param name="orientation">Orientation.</param>
    /// <returns>Unit vector.</returns>
    /// <exception cref="ArgumentException">Thrown if an invalid orientation is passed.</exception>
    public static Vector3 GetUnitVector(Orientation orientation)
    {
        var diag = 0.707106781186f; // 1/sqrt(2)
        return orientation switch
        {
            Orientation.South => new Vector3(0.0f, -1.0f, 0.0f),
            Orientation.Southeast => new Vector3(diag, -diag, 0.0f),
            Orientation.East => new Vector3(1.0f, 0.0f, 0.0f),
            Orientation.Northeast => new Vector3(diag, diag, 0.0f),
            Orientation.North => new Vector3(0.0f, 1.0f, 0.0f),
            Orientation.Northwest => new Vector3(-diag, diag, 0.0f),
            Orientation.West => new Vector3(-1.0f, 0.0f, 0.0f),
            Orientation.Southwest => new Vector3(-diag, -diag, 0.0f),
            _ => throw new ArgumentException("Unrecognized orientation.")
        };
    }
}