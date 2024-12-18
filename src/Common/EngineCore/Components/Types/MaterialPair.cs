// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
///     Represents a material identifier as a pair of material ID and
///     material modifier.
/// </summary>
[MessagePackObject]
[Scriptable]
public class MaterialPair
{
    public MaterialPair()
    {
    }

    public MaterialPair(int materialId, int materialModifier)
    {
        MaterialId = materialId;
        MaterialModifier = materialModifier;
    }

    /// <summary>
    ///     Material ID.
    /// </summary>
    [Key(0)]
    [ScriptableField]
    public int MaterialId { get; set; }

    /// <summary>
    ///     Material modifier.
    /// </summary>
    [Key(1)]
    [ScriptableField]
    public int MaterialModifier { get; set; }
}