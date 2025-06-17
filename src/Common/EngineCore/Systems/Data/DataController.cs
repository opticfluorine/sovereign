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

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Controller interface for the Data system.
/// </summary>
public interface IDataController
{
    /// <summary>
    ///     Synchronously sets a global key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to string.</exception>
    void SetGlobalSync<T>(string key, T value) where T : notnull;

    /// <summary>
    ///     Synchronously removes a global key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    void RemoveGlobalSync(string key);

    /// <summary>
    ///     Sets a key-value pair for an entity synchronously.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull;

    /// <summary>
    ///     Removes a key-value pair from an entity synchronously.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    void RemoveEntityKeyValueSync(ulong entityId, string key);
}

/// <summary>
///     Implementation of the IDataController interface.
/// </summary>
internal class DataController(GlobalKeyValueStore globalStore, EntityKeyValueStore entityKeyValueStore)
    : IDataController
{
    public void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull
    {
        entityKeyValueStore.SetValue(entityId, key, value);
    }

    public void RemoveEntityKeyValueSync(ulong entityId, string key)
    {
        entityKeyValueStore.RemoveKey(entityId, key);
    }

    public void SetGlobalSync<T>(string key, T value) where T : notnull
    {
        var stringValue = value.ToString() ?? throw new InvalidCastException();
        globalStore.SetGlobal(key, stringValue);
    }

    public void RemoveGlobalSync(string key)
    {
        globalStore.RemoveGlobal(key);
    }
}