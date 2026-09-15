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
///     Describes a radiant scalar field contribution at the position of the associated entity.
/// </summary>
[MessagePackObject]
[Scriptable]
public struct RadiantData
{
    /// <summary>
    ///     Category of the radiant field.
    /// </summary>
    [ScriptableField]
    [Key(0)]
    public RadiantCategory Category { get; set; }

    /// <summary>
    ///     Function used to evaluate the field contribution.
    /// </summary>
    [ScriptableField]
    [Key(1)]
    public RadiantFunction Function { get; set; }

    /// <summary>
    ///     First function parameter.
    /// </summary>
    [ScriptableField]
    [Key(2)]
    public float Param0 { get; set; }

    /// <summary>
    ///     Second function parameter.
    /// </summary>
    [ScriptableField]
    [Key(3)]
    public float Param1 { get; set; }

    /// <summary>
    ///     Third function parameter.
    /// </summary>
    [ScriptableField]
    [Key(4)]
    public float Param2 { get; set; }
}
