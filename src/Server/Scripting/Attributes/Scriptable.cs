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

namespace Sovereign.Scripting.Attributes;

/// <summary>
///     Attribute used to indicate that a type may be passed to and from Lua scripts.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class Scriptable : Attribute
{
}

/// <summary>
///     Attribute used to indicate that a field or property is to be marshalled to Lua.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ScriptableOrder : Attribute
{
    /// <summary>
    ///     Indicates that a field or property is to be marshalled to Lua.
    /// </summary>
    /// <param name="index">Parameter index for this field or property. Must be unique per class/struct.</param>
    public ScriptableOrder(uint index)
    {
        Index = index;
    }

    /// <summary>
    ///     Order in which this field or property will be marshalled (0 = first).
    /// </summary>
    public uint Index { get; }
}