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

namespace Sovereign.EngineCore.Configuration;

/// <summary>
///     Configurable options for the combat system.
/// </summary>
public sealed class CombatOptions
{
    /// <summary>
    ///     Default attack range in world units for attackers without an AttackDetails component.
    /// </summary>
    public float DefaultAttackRange { get; set; } = 1.0f;

    /// <summary>
    ///     Default minimum time between attacks in microseconds for attackers without an AttackDetails component.
    /// </summary>
    public uint DefaultAttackDelayUs { get; set; } = 250000;
}
