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

using System;

namespace Sovereign.EngineUtil.Attributes;

/// <summary>
///     Attribute used to indicate that a type may be passed to and from Lua scripts.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class Scriptable : Attribute
{
}

/// <summary>
///     Attribute used to indicate that a field or property is to be marshalled to Lua.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ScriptableField : Attribute
{
}

/// <summary>
///     Designates a class as being intended for export to Lua as a library.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ScriptableLibrary : Attribute
{
    public ScriptableLibrary(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
///     Designates a method in a [ScriptableLibrary] class as a target for export to Lua.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ScriptableFunction : Attribute
{
    public ScriptableFunction(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
///     Designates an event ID as an event which a script can react to.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ScriptableEvent : Attribute
{
    public ScriptableEvent(string? detailsClass = null)
    {
    }
}

/// <summary>
///     Designates an enum to be bound to the scripting engine.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class ScriptableEnum : Attribute
{
}

/// <summary>
///     Designates a component collection class as one for which Lua bindings should be generated.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ScriptableComponents : Attribute
{
    public ScriptableComponents(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

/// <summary>
///     Designates an IEntityBuilder method for binding to Lua.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ScriptableEntityBuilderAction : Attribute
{
    public ScriptableEntityBuilderAction(string keyName)
    {
        KeyName = keyName;
    }

    public string KeyName { get; }
}