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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Per-entity key-value store.
/// </summary>
internal class EntityKeyValueStore
{
    private readonly EntityTable entityTable;
    private readonly IEventSender eventSender;
    private readonly DataInternalController internalController;
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<string, DataValue>> keyValueStores = new();

    /// <summary>
    ///     Entities removed since the last database synchronization.
    /// </summary>
    private readonly List<ulong> removedEntities = new();

    public EntityKeyValueStore(EntityTable entityTable, IEventSender eventSender,
        DataInternalController internalController)
    {
        this.entityTable = entityTable;
        this.eventSender = eventSender;
        this.internalController = internalController;
        entityTable.OnNonBlockEntityRemoved += OnNonBlockEntityRemoved;
    }

    /// <summary>
    ///     Called when database synchronization is complete.
    /// </summary>
    public void OnSynchronizationComplete()
    {
        foreach (var entityId in removedEntities) keyValueStores.TryRemove(entityId, out _);

        removedEntities.Clear();
    }

    /// <summary>
    ///     Gets the value (if any) for the given entity ID and key. Also checks the template entity (if any)
    ///     if the key is not found.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if method returns true.</param>
    /// <returns>true if the key-value pair exists for the entity, false otherwise.</returns>
    public bool TryGetValue(ulong entityId, string key, out DataValue value)
    {
        value = default;

        // First try to get the key-value pair for the entity directly.
        if (keyValueStores.TryGetValue(entityId, out var store))
            if (store.TryGetValue(key, out value))
                return true;

        // If we didn't find it, check the entity's template as well.
        if (!entityTable.TryGetTemplate(entityId, out var templateId)) return false;
        if (!keyValueStores.TryGetValue(templateId, out store)) return false;
        return store.TryGetValue(key, out value);
    }

    /// <summary>
    ///     Gets the value (if any) for the given entity ID and key, checking only the entity's own
    ///     store without the template entity fallback.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if method returns true.</param>
    /// <returns>true if the key-value pair exists for the entity, false otherwise.</returns>
    public bool TryGetValueLocal(ulong entityId, string key, out DataValue value)
    {
        if (keyValueStores.TryGetValue(entityId, out var store) && store.TryGetValue(key, out value))
            return true;

        value = default;
        return false;
    }

    /// <summary>
    ///     Sets the value for the given entity ID and key.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public void SetValue(ulong entityId, string key, DataValue value)
    {
        if (!keyValueStores.TryGetValue(entityId, out var store))
        {
            store = new ConcurrentDictionary<string, DataValue>();
            keyValueStores[entityId] = store;
        }

        store[key] = value;

        lock (eventSender)
        {
            internalController.EntityKeyValueSet(eventSender, entityId, key, value);
        }
    }

    /// <summary>
    ///     Sets an enum-typed value for the given entity ID and key. Enums are stored as Int values.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Enum value.</param>
    /// <typeparam name="T">Enum type.</typeparam>
    public void SetValue<T>(ulong entityId, string key, T value) where T : struct, Enum
    {
        SetValue(entityId, key, DataValue.FromInt(Convert.ToInt64(value)));
    }

    /// <summary>
    ///     Removes the given key from the entity's key-value store if it is present.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    public void RemoveKey(ulong entityId, string key)
    {
        if (!keyValueStores.TryGetValue(entityId, out var store)) return;
        if (!store.TryRemove(key, out _)) return;

        lock (eventSender)
        {
            internalController.EntityKeyValueRemoved(eventSender, entityId, key);
        }
    }

    /// <summary>
    ///     Removes all key-value pairs for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void ClearKeyValues(ulong entityId)
    {
        if (!keyValueStores.TryGetValue(entityId, out var store)) return;

        lock (eventSender)
        {
            foreach (var key in store.Keys) internalController.EntityKeyValueRemoved(eventSender, entityId, key);
        }

        store.Clear();
    }

    /// <summary>
    ///     Adds all key-values for the given entity to the provided dictionary.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="data">Dictionary to append with entity key-value pairs, if any.</param>
    public void GetAllKeyValuePairs(ulong entityId, Dictionary<string, DataValue> data)
    {
        if (!keyValueStores.TryGetValue(entityId, out var store)) return;
        foreach (var kv in store)
            data[kv.Key] = kv.Value;
    }

    /// <summary>
    ///     Called when a non-block entity is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void OnNonBlockEntityRemoved(ulong entityId)
    {
        removedEntities.Add(entityId);
    }
}
