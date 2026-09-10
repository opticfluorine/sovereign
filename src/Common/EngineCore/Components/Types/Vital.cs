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
///     Combined structure for vital component data.
/// </summary>
[MessagePackObject]
[Scriptable]
public struct Vital
{
    /// <summary>
    ///     Current value of the vital.
    /// </summary>
    [Key(0)]
    [ScriptableField] public int Value;

    /// <summary>
    ///     Maximum value of the vital.
    /// </summary>
    [Key(1)]
    [ScriptableField] public int MaxValue;

    /// <summary>
    ///     Amount added to the value on each change interval.
    /// </summary>
    [Key(2)]
    [ScriptableField] public int ChangeRate;

    /// <summary>
    ///     Interval in ticks between changes, or zero to disable periodic changes.
    /// </summary>
    [Key(3)]
    [ScriptableField] public uint ChangeInterval;
}
