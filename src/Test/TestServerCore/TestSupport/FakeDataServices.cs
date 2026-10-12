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
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.ServerCore.TestSupport;

/// <summary>
///     Test double for IDataServices backed by an in-memory global key-value store.
/// </summary>
public class FakeDataServices : IDataServices
{
    private readonly Dictionary<string, string> globals = new();

    /// <summary>
    ///     Sets a global key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void SetGlobal(string key, string value)
    {
        globals[key] = value;
    }

    public bool HasGlobal(string key)
    {
        return globals.ContainsKey(key);
    }

    public bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value)
    {
        return globals.TryGetValue(key, out value);
    }

    public bool TryGetGlobalInt(string key, out int value)
    {
        value = 0;
        return TryGetGlobal(key, out var raw) && int.TryParse(raw, out value);
    }

    public bool TryGetGlobalUlong(string key, out ulong value)
    {
        value = 0;
        return TryGetGlobal(key, out var raw) && ulong.TryParse(raw, out value);
    }

    public bool TryGetGlobalFloat(string key, out float value)
    {
        value = 0.0f;
        return TryGetGlobal(key, out var raw) && float.TryParse(raw, out value);
    }

    public bool TryGetGlobalBool(string key, out bool value)
    {
        value = false;
        return TryGetGlobal(key, out var raw) && bool.TryParse(raw, out value);
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

    public void GetEntityData(ulong entityId, Dictionary<string, string> data)
    {
        data.Clear();
    }

    public bool IsKeyReadOnly(string key)
    {
        return key.StartsWith("__");
    }
}
