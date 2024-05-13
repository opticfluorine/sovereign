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

using System.Numerics;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Combined structure for kinematic component data.
/// </summary>
public struct Kinematics
{
    /// <summary>
    ///     Position.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    ///     Velocity in tiles per second.
    /// </summary>
    public Vector3 Velocity { get; set; }
}