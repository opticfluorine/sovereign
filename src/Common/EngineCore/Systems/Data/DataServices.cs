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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Interface for read-only services exposed by the Data system.
/// </summary>
/// <remarks>
///     Implementations of this interface must be thread-safe.
/// </remarks>
public interface IDataServices
{
    /// <summary>
    ///     Checks if the given global key exists.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <returns>true if exists, false otherwise.</returns>
    bool HasGlobal(string key);

    /// <summary>
    ///     Gets the string value associated with the given global key.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value, or null if the method returns false.</param>
    /// <returns>true if a matching key was found, false otherwise.</returns>
    bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    ///     Gets the value associated with the given global key as an integer.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found and the value can be converted; false otherwise.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to integer.</exception>
    bool TryGetGlobalInt(string key, out int value);

    /// <summary>
    ///     Gets the value associated with the given global key as an ulong.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>Associated ulong value.</returns>
    /// <returns>true if the key is found and the value can be converted; false otherwise.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to ulong.</exception>
    bool TryGetGlobalUlong(string key, out ulong value);

    /// <summary>
    ///     Gets the value associated with the given global key as an float.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found and the value can be converted; false otherwise.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to float.</exception>
    bool TryGetGlobalFloat(string key, out float value);

    /// <summary>
    ///     Gets the value associated with the given global key as a bool.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found and the value can be converted; false otherwise.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to bool.</exception>
    bool TryGetGlobalBool(string key, out bool value);

    /// <summary>
    ///     Gets a key-value pair for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found for the entity, false otherwise.</returns>
    bool TryGetEntityKeyValue(ulong entityId, string key, [NotNullWhen(true)] out string? value);
}

/// <summary>
///     Implementation of IDataServices.
/// </summary>
internal class DataServices : IDataServices
{
    private readonly EntityKeyValueStore entityStore;
    private readonly GlobalKeyValueStore globalStore;

    public DataServices(GlobalKeyValueStore globalStore, EntityKeyValueStore entityStore)
    {
        this.globalStore = globalStore;
        this.entityStore = entityStore;
    }

    public bool HasGlobal(string key)
    {
        return globalStore.KeyValueStore.ContainsKey(key);
    }

    public bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value)
    {
        return globalStore.KeyValueStore.TryGetValue(key, out value);
    }

    public bool TryGetGlobalInt(string key, out int value)
    {
        value = 0;
        return TryGetGlobal(key, out var rawValue) && int.TryParse(rawValue, out value);
    }

    public bool TryGetGlobalUlong(string key, out ulong value)
    {
        value = 0;
        return TryGetGlobal(key, out var rawValue) && ulong.TryParse(rawValue, out value);
    }

    public bool TryGetGlobalFloat(string key, out float value)
    {
        value = 0.0f;
        return TryGetGlobal(key, out var rawValue) && float.TryParse(rawValue, out value);
    }

    public bool TryGetGlobalBool(string key, out bool value)
    {
        value = false;
        return TryGetGlobal(key, out var rawValue) && bool.TryParse(rawValue, out value);
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, [NotNullWhen(true)] out string? value)
    {
        return entityStore.TryGetValue(entityId, key, out value);
    }
}