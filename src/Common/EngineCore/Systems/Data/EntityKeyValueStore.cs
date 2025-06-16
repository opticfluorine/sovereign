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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineCore.Events;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Per-entity key-value store.
/// </summary>
internal class EntityKeyValueStore
{
    private readonly IEventSender eventSender;
    private readonly DataInternalController internalController;
    private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<string, string>> keyValueStores = new();

    /// <summary>
    ///     Entities removed since the last database synchronization.
    /// </summary>
    private readonly List<ulong> removedEntities = new();

    public EntityKeyValueStore(EntityTable entityTable, IEventSender eventSender,
        DataInternalController internalController)
    {
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
    ///     Gets the value (if any) for the given entity ID and key.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value. Only meaningful if method returns true.</param>
    /// <returns>true if the key-value pair exists for the entity, false otherwise.</returns>
    public bool TryGetValue(ulong entityId, string key, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!keyValueStores.TryGetValue(entityId, out var store)) return false;
        return store.TryGetValue(key, out value);
    }

    /// <summary>
    ///     Sets the value for the given entity ID and key.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    public void SetValue<T>(ulong entityId, string key, T value) where T : notnull
    {
        if (!keyValueStores.TryGetValue(entityId, out var store))
        {
            store = new ConcurrentDictionary<string, string>();
            keyValueStores[entityId] = store;
        }

        var stringValue = value.ToString() ?? string.Empty;
        store[key] = stringValue;

        lock (eventSender)
        {
            internalController.EntityKeyValueSet(eventSender, entityId, key, stringValue);
        }
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
    ///     Called when a non-block entity is removed or unloaded.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    private void OnNonBlockEntityRemoved(ulong entityId)
    {
        removedEntities.Add(entityId);
    }
}