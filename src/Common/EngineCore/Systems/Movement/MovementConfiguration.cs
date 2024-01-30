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

namespace Sovereign.EngineCore.Systems.Movement;

/// <summary>
///     Configuration for the Movement system.
/// </summary>
public class MovementConfiguration
{
    /// <summary>
    ///     Default length in ticks of a movement interval.
    /// </summary>
    public const int DefaultMovementLengthTicks = 10;

    /// <summary>
    ///     Default base velocity for all entities.
    /// </summary>
    public static readonly Vector3 DefaultBaseVelocity = new(4.0f, 4.0f, 0.0f);
}