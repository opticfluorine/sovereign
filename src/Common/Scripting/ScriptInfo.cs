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

namespace Sovereign.Scripting;

/// <summary>
///     Enum describing the type of an entity parameter.
/// </summary>
public enum EntityParameterType
{
    /// <summary>
    ///     String-valued parameter.
    /// </summary>
    String,

    /// <summary>
    ///     Integer-valued parameter.
    /// </summary>
    Int,

    /// <summary>
    ///     Float-valued parameter.
    /// </summary>
    Float,

    /// <summary>
    ///     Boolean-valued parameter.
    /// </summary>
    Bool
}

/// <summary>
///     Data class describing the interfaces exposed by the currently loaded scripts.
/// </summary>
public sealed class ScriptInfo
{
    /// <summary>
    ///     Currently loaded scripts.
    /// </summary>
    public List<SingleScriptInfo> Scripts { get; set; } = new();
}

/// <summary>
///     Data class describing the interfaces exposed by a single loaded script.
/// </summary>
public sealed class SingleScriptInfo
{
    /// <summary>
    ///     Script naem.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Global functions exposed by the script.
    /// </summary>
    public List<ScriptFunctionInfo> Functions { get; set; } = new();
}

/// <summary>
///     Data class describing a function exposed by a script.
/// </summary>
public sealed class ScriptFunctionInfo
{
    /// <summary>
    ///     Function name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Hints for entity key-value pairs that the function references.
    /// </summary>
    public List<EntityParameterHint> EntityParameters { get; set; } = new();
}

/// <summary>
///     Describes a single entity parameter hint.
/// </summary>
public sealed class EntityParameterHint
{
    /// <summary>
    ///     Parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Parameter type.
    /// </summary>
    public EntityParameterType Type { get; set; } = EntityParameterType.String;

    /// <summary>
    ///     Parameter tooltip.
    /// </summary>
    public string Tooltip { get; set; } = string.Empty;
}