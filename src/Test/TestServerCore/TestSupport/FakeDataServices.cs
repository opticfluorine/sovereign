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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.TestSupport;

/// <summary>
///     Test double for IDataServices backed by an in-memory global key-value store.
/// </summary>
public class FakeDataServices : IDataServices
{
    private readonly Dictionary<string, DataValue> globals = new();

    /// <summary>
    ///     Sets a global key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void SetGlobal(string key, DataValue value)
    {
        globals[key] = value;
    }

    public bool HasGlobal(string key)
    {
        return globals.ContainsKey(key);
    }

    public bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value)
    {
        if (globals.TryGetValue(key, out var dataValue))
        {
            value = dataValue.ToString();
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetGlobal(string key, out DataValue value)
    {
        return globals.TryGetValue(key, out value);
    }

    public bool TryGetGlobal(string key, DataValueType type, out DataValue value)
    {
        if (!globals.TryGetValue(key, out value) || value.Type != type)
        {
            value = default;
            return false;
        }

        return true;
    }

    public bool TryGetGlobalInt(string key, out int value)
    {
        value = 0;
        if (!TryGetGlobal(key, DataValueType.Int, out var dataValue)) return false;
        value = (int)dataValue.AsInt();
        return true;
    }

    public bool TryGetGlobalBool(string key, out bool value)
    {
        value = false;
        if (!TryGetGlobal(key, DataValueType.Bool, out var dataValue)) return false;
        value = dataValue.AsBool();
        return true;
    }

    public bool TryGetGlobalFloat(string key, out float value)
    {
        value = 0.0f;
        if (!TryGetGlobal(key, DataValueType.Float, out var dataValue)) return false;
        value = (float)dataValue.AsFloat();
        return true;
    }

    public bool TryGetGlobalVector3(string key, out Vector3 value)
    {
        value = Vector3.Zero;
        if (!TryGetGlobal(key, DataValueType.Vector3, out var dataValue)) return false;
        value = dataValue.AsVector3();
        return true;
    }

    public bool TryGetGlobalEntityId(string key, out ulong value)
    {
        value = 0;
        if (!TryGetGlobal(key, DataValueType.EntityId, out var dataValue)) return false;
        value = dataValue.AsEntityId();
        return true;
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    public bool TryGetEntityKeyValueLocal(ulong entityId, string key, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, out DataValue value)
    {
        value = default;
        return false;
    }

    public bool TryGetEntityKeyValueLocal(ulong entityId, string key, out DataValue value)
    {
        value = default;
        return false;
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, DataValueType type, out DataValue value)
    {
        value = default;
        return false;
    }

    public bool TryGetEntityKeyValueInt(ulong entityId, string key, out int value)
    {
        value = 0;
        return false;
    }

    public bool TryGetEntityKeyValueBool(ulong entityId, string key, out bool value)
    {
        value = false;
        return false;
    }

    public bool TryGetEntityKeyValueFloat(ulong entityId, string key, out float value)
    {
        value = 0.0f;
        return false;
    }

    public bool TryGetEntityKeyValueVector3(ulong entityId, string key, out Vector3 value)
    {
        value = Vector3.Zero;
        return false;
    }

    public bool TryGetEntityKeyValueEntityId(ulong entityId, string key, out ulong value)
    {
        value = 0;
        return false;
    }

    public void GetEntityData(ulong entityId, Dictionary<string, DataValue> data)
    {
        data.Clear();
    }

    public bool IsKeyReadOnly(string key)
    {
        return key.StartsWith("__");
    }
}
