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

using MessagePack;
using Sovereign.EngineUtil.Attributes;

namespace Sovereign.EngineCore.Components.Types;

/// <summary>
///     Combined structure for the basic statistics of an entity.
/// </summary>
[MessagePackObject]
[Scriptable]
public struct Stats
{
    /// <summary>
    ///     Physical strength of the entity.
    /// </summary>
    [Key(0)]
    [ScriptableField] public int Strength;

    /// <summary>
    ///     Physical defense of the entity.
    /// </summary>
    [Key(1)]
    [ScriptableField] public int Defense;

    /// <summary>
    ///     Agility of the entity.
    /// </summary>
    [Key(2)]
    [ScriptableField] public int Agility;

    /// <summary>
    ///     Intelligence of the entity.
    /// </summary>
    [Key(3)]
    [ScriptableField] public int Intelligence;

    /// <summary>
    ///     Wisdom of the entity.
    /// </summary>
    [Key(4)]
    [ScriptableField] public int Wisdom;

    /// <summary>
    ///     Charisma of the entity.
    /// </summary>
    [Key(5)]
    [ScriptableField] public int Charisma;

    /// <summary>
    ///     Luck of the entity.
    /// </summary>
    [Key(6)]
    [ScriptableField] public int Luck;
}
