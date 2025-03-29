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
///     Constants related to game physics.
/// </summary>
public static class PhysicsConstants
{
    /// <summary>
    ///     Gravitational acceleration along z axis in world units per sec^2.
    /// </summary>
    public const float GravityAcceleration = -1.0f;

    /// <summary>
    ///     Maximum falling velocity in world units per second.
    /// </summary>
    public const float TerminalVelocity = 4.0f;

    /// <summary>
    ///     Initial jump velocity in world units per second.
    /// </summary>
    public const float InitialJumpVelocity = 4.0f;
}