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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Interface for read-only services exposed by the Data system.
/// </summary>
/// <remarks>
///     Implementations of this interface must be thread-safe. Typed getters require an exact type
///     tag match and never coerce values between types. String getters return the canonical text
///     encoding of any value.
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
    ///     Gets the canonical text encoding of the value associated with the given global key.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value, or null if the method returns false.</param>
    /// <returns>true if a matching key was found, false otherwise.</returns>
    bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    ///     Gets the tagged value associated with the given global key.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if a matching key was found, false otherwise.</returns>
    bool TryGetGlobal(string key, out DataValue value);

    /// <summary>
    ///     Gets the tagged value associated with the given global key, requiring the given type tag.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="type">Required type tag.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with a matching tag, false otherwise.</returns>
    bool TryGetGlobal(string key, DataValueType type, out DataValue value);

    /// <summary>
    ///     Gets the value associated with the given global key as an integer.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with an Int tag, false otherwise.</returns>
    bool TryGetGlobalInt(string key, out int value);

    /// <summary>
    ///     Gets the value associated with the given global key as a bool.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with a Bool tag, false otherwise.</returns>
    bool TryGetGlobalBool(string key, out bool value);

    /// <summary>
    ///     Gets the value associated with the given global key as a float.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with a Float tag, false otherwise.</returns>
    bool TryGetGlobalFloat(string key, out float value);

    /// <summary>
    ///     Gets the value associated with the given global key as a Vector3.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with a Vector3 tag, false otherwise.</returns>
    bool TryGetGlobalVector3(string key, out Vector3 value);

    /// <summary>
    ///     Gets the value associated with the given global key as an entity ID.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <param name="value">Associated value. Only valid if the method returns true.</param>
    /// <returns>true if the key is found with an EntityId tag, false otherwise.</returns>
    bool TryGetGlobalEntityId(string key, out ulong value);

    /// <summary>
    ///     Gets the canonical text encoding of a key-value pair for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found for the entity, false otherwise.</returns>
    bool TryGetEntityKeyValue(ulong entityId, string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    ///     Gets the canonical text encoding of a key-value pair for the given entity, checking only
    ///     the entity's own store without the template entity fallback.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found for the entity, false otherwise.</returns>
    bool TryGetEntityKeyValueLocal(ulong entityId, string key, [NotNullWhen(true)] out string? value);

    /// <summary>
    ///     Gets the tagged value for the given entity's key-value pair.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found for the entity, false otherwise.</returns>
    bool TryGetEntityKeyValue(ulong entityId, string key, out DataValue value);

    /// <summary>
    ///     Gets the tagged value for the given entity's key-value pair, checking only the entity's
    ///     own store without the template entity fallback.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found for the entity, false otherwise.</returns>
    bool TryGetEntityKeyValueLocal(ulong entityId, string key, out DataValue value);

    /// <summary>
    ///     Gets the tagged value for the given entity's key-value pair, requiring the given type tag.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="type">Required type tag.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with a matching tag, false otherwise.</returns>
    bool TryGetEntityKeyValue(ulong entityId, string key, DataValueType type, out DataValue value);

    /// <summary>
    ///     Gets the value for the given entity's key-value pair as an integer.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with an Int tag, false otherwise.</returns>
    bool TryGetEntityKeyValueInt(ulong entityId, string key, out int value);

    /// <summary>
    ///     Gets the value for the given entity's key-value pair as a bool.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with a Bool tag, false otherwise.</returns>
    bool TryGetEntityKeyValueBool(ulong entityId, string key, out bool value);

    /// <summary>
    ///     Gets the value for the given entity's key-value pair as a float.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with a Float tag, false otherwise.</returns>
    bool TryGetEntityKeyValueFloat(ulong entityId, string key, out float value);

    /// <summary>
    ///     Gets the value for the given entity's key-value pair as a Vector3.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with a Vector3 tag, false otherwise.</returns>
    bool TryGetEntityKeyValueVector3(ulong entityId, string key, out Vector3 value);

    /// <summary>
    ///     Gets the value for the given entity's key-value pair as an entity ID.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if the method returns true.</param>
    /// <returns>true if the key is found with an EntityId tag, false otherwise.</returns>
    bool TryGetEntityKeyValueEntityId(ulong entityId, string key, out ulong value);

    /// <summary>
    ///     Gets all key-value pairs for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="data">Dictionary that will be cleared and filled with all key-value pairs.</param>
    void GetEntityData(ulong entityId, Dictionary<string, DataValue> data);

    /// <summary>
    ///     Determines whether a key is read-only to scripts and admin chat commands.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>true if the key is read-only, false otherwise.</returns>
    bool IsKeyReadOnly(string key);
}

/// <summary>
///     Implementation of IDataServices.
/// </summary>
internal class DataServices : IDataServices
{
    private readonly EntityKeyValueStore entityStore;
    private readonly GlobalKeyValueStore globalStore;
    private readonly ILogger<DataServices> logger;

    public DataServices(GlobalKeyValueStore globalStore, EntityKeyValueStore entityStore,
        ILogger<DataServices> logger)
    {
        this.globalStore = globalStore;
        this.entityStore = entityStore;
        this.logger = logger;
    }

    public bool HasGlobal(string key)
    {
        return globalStore.KeyValueStore.ContainsKey(key);
    }

    public bool TryGetGlobal(string key, [NotNullWhen(true)] out string? value)
    {
        if (globalStore.KeyValueStore.TryGetValue(key, out var dataValue))
        {
            value = dataValue.ToString();
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetGlobal(string key, out DataValue value)
    {
        return globalStore.KeyValueStore.TryGetValue(key, out value);
    }

    public bool TryGetGlobal(string key, DataValueType type, out DataValue value)
    {
        if (!globalStore.KeyValueStore.TryGetValue(key, out value)) return false;
        if (value.Type == type) return true;

        LogTypeMismatch(key, value.Type, type);
        value = default;
        return false;
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
        if (entityStore.TryGetValue(entityId, key, out var dataValue))
        {
            value = dataValue.ToString();
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetEntityKeyValueLocal(ulong entityId, string key, [NotNullWhen(true)] out string? value)
    {
        if (entityStore.TryGetValueLocal(entityId, key, out var dataValue))
        {
            value = dataValue.ToString();
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, out DataValue value)
    {
        return entityStore.TryGetValue(entityId, key, out value);
    }

    public bool TryGetEntityKeyValueLocal(ulong entityId, string key, out DataValue value)
    {
        return entityStore.TryGetValueLocal(entityId, key, out value);
    }

    public bool TryGetEntityKeyValue(ulong entityId, string key, DataValueType type, out DataValue value)
    {
        if (!entityStore.TryGetValue(entityId, key, out value)) return false;
        if (value.Type == type) return true;

        LogTypeMismatch(key, value.Type, type, entityId);
        value = default;
        return false;
    }

    public bool TryGetEntityKeyValueInt(ulong entityId, string key, out int value)
    {
        value = 0;
        if (!TryGetEntityKeyValue(entityId, key, DataValueType.Int, out var dataValue)) return false;
        value = (int)dataValue.AsInt();
        return true;
    }

    public bool TryGetEntityKeyValueBool(ulong entityId, string key, out bool value)
    {
        value = false;
        if (!TryGetEntityKeyValue(entityId, key, DataValueType.Bool, out var dataValue)) return false;
        value = dataValue.AsBool();
        return true;
    }

    public bool TryGetEntityKeyValueFloat(ulong entityId, string key, out float value)
    {
        value = 0.0f;
        if (!TryGetEntityKeyValue(entityId, key, DataValueType.Float, out var dataValue)) return false;
        value = (float)dataValue.AsFloat();
        return true;
    }

    public bool TryGetEntityKeyValueVector3(ulong entityId, string key, out Vector3 value)
    {
        value = Vector3.Zero;
        if (!TryGetEntityKeyValue(entityId, key, DataValueType.Vector3, out var dataValue)) return false;
        value = dataValue.AsVector3();
        return true;
    }

    public bool TryGetEntityKeyValueEntityId(ulong entityId, string key, out ulong value)
    {
        value = 0;
        if (!TryGetEntityKeyValue(entityId, key, DataValueType.EntityId, out var dataValue)) return false;
        value = dataValue.AsEntityId();
        return true;
    }

    public void GetEntityData(ulong entityId, Dictionary<string, DataValue> data)
    {
        data.Clear();
        entityStore.GetAllKeyValuePairs(entityId, data);
    }

    public bool IsKeyReadOnly(string key)
    {
        return DataKeyConstraints.IsKeyReadOnly(key);
    }

    /// <summary>
    ///     Logs a typed-get type mismatch.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="storedType">Stored type tag.</param>
    /// <param name="requestedType">Requested type tag.</param>
    private void LogTypeMismatch(string key, DataValueType storedType, DataValueType requestedType)
    {
        logger.LogError("Data key {Key} has stored type {StoredType} but {RequestedType} was requested.", key,
            storedType, requestedType);
    }

    /// <summary>
    ///     Logs a typed-get type mismatch for an entity key.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="storedType">Stored type tag.</param>
    /// <param name="requestedType">Requested type tag.</param>
    /// <param name="entityId">Entity ID.</param>
    private void LogTypeMismatch(string key, DataValueType storedType, DataValueType requestedType, ulong entityId)
    {
        logger.LogError(
            "Entity {EntityId:X16} data key {Key} has stored type {StoredType} but {RequestedType} was requested.",
            entityId, key, storedType, requestedType);
    }
}
