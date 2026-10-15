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
using System.Numerics;

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
    /// <exception cref="InvalidCastException">Thrown if the value type is not supported.</exception>
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
    /// <exception cref="InvalidCastException">Thrown if the value type is not supported.</exception>
    void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull;

    /// <summary>
    ///     Removes a key-value pair from an entity synchronously.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    void RemoveEntityKeyValueSync(ulong entityId, string key);

    /// <summary>
    ///     Removes all key-value pairs for an entity synchronously.
    /// </summary>
    /// <param name="entityId"></param>
    void ClearEntityKeyValuesSync(ulong entityId);
}

/// <summary>
///     Implementation of the IDataController interface.
/// </summary>
internal class DataController(GlobalKeyValueStore globalStore, EntityKeyValueStore entityKeyValueStore)
    : IDataController
{
    public void SetEntityKeyValueSync<T>(ulong entityId, string key, T value) where T : notnull
    {
        entityKeyValueStore.SetValue(entityId, key, ToDataValue(value));
    }

    public void RemoveEntityKeyValueSync(ulong entityId, string key)
    {
        entityKeyValueStore.RemoveKey(entityId, key);
    }

    public void ClearEntityKeyValuesSync(ulong entityId)
    {
        entityKeyValueStore.ClearKeyValues(entityId);
    }

    public void SetGlobalSync<T>(string key, T value) where T : notnull
    {
        globalStore.SetGlobal(key, ToDataValue(value));
    }

    public void RemoveGlobalSync(string key)
    {
        globalStore.RemoveGlobal(key);
    }

    /// <summary>
    ///     Converts a supported CLR value into a tagged <see cref="DataValue" />.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Tagged value.</returns>
    /// <exception cref="InvalidCastException">Thrown if the value type is not supported.</exception>
    private static DataValue ToDataValue<T>(T value) where T : notnull
    {
        switch (value)
        {
            case DataValue dataValue:
                return dataValue;
            case string stringValue:
                return DataValue.FromString(stringValue);
            case bool boolValue:
                return DataValue.FromBool(boolValue);
            case int intValue:
                return DataValue.FromInt(intValue);
            case long longValue:
                return DataValue.FromInt(longValue);
            case short shortValue:
                return DataValue.FromInt(shortValue);
            case byte byteValue:
                return DataValue.FromInt(byteValue);
            case uint uintValue:
                return DataValue.FromInt(uintValue);
            case ulong ulongValue:
                return DataValue.FromEntityId(ulongValue);
            case float floatValue:
                return DataValue.FromFloat(floatValue);
            case double doubleValue:
                return DataValue.FromFloat(doubleValue);
            case Vector3 vectorValue:
                return DataValue.FromVector3(vectorValue);
            case Enum enumValue:
                return DataValue.FromInt(Convert.ToInt64(enumValue));
            default:
                throw new InvalidCastException($"Unsupported data value type {typeof(T)}.");
        }
    }
}
