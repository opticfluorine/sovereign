/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.Entities;

/// <summary>
///     Maps persisted entity IDs to volatile entity IDs and vice versa.
/// </summary>
public sealed class EntityMapper
{
    private readonly IDictionary<ulong, ulong> persistedToVolatile
        = new Dictionary<ulong, ulong>();

    private readonly IDictionary<ulong, ulong> volatileToPersisted
        = new Dictionary<ulong, ulong>();

    /// <summary>
    ///     Whether the subsystem has been initialized.
    /// </summary>
    private bool initialized;

    private INextPersistedIdQuery? nextIdQuery;

    /// <summary>
    ///     Next available persisted ID.
    /// </summary>
    public ulong NextPersistedId { get; private set; }

    /// <summary>
    ///     Initializes the mapper to generate unique persisted IDs.
    /// </summary>
    public void InitializeMapper(INextPersistedIdQuery nextIdQuery)
    {
        this.nextIdQuery = nextIdQuery;
        NextPersistedId = nextIdQuery.GetNextPersistedEntityId();

        initialized = true;
    }

    /// <summary>
    ///     Gets the persisted entity ID for the given volatile entity ID.
    /// </summary>
    /// <param name="volatileEntityId">Volatile entity ID.</param>
    /// <param name="needToCreate">
    ///     Set to true if the entity ID needs to
    ///     be added to the database.
    /// </param>
    /// <returns>Persisted entity ID.</returns>
    public ulong GetPersistedId(ulong volatileEntityId, out bool needToCreate)
    {
        if (!initialized) throw new InvalidOperationException("Not initialized");

        needToCreate = false;
        if (volatileEntityId >= EntityAssigner.FirstPersistedId) return volatileEntityId;

        if (volatileToPersisted.ContainsKey(volatileEntityId)) return volatileToPersisted[volatileEntityId];

        needToCreate = true;
        return GetNewPersistedId(volatileEntityId);
    }

    /// <summary>
    ///     Gets the volatile entity ID for the given persisted entity ID.
    /// </summary>
    /// <param name="persistedEntityId">Persisted entity ID.</param>
    /// <returns>Volatile entity ID.</returns>
    /// <remarks>
    ///     If no volatile entity ID is known for the persisted ID, this method
    ///     returns the persisted ID.
    /// </remarks>
    public ulong GetVolatileId(ulong persistedEntityId)
    {
        if (!initialized) throw new InvalidOperationException("Not initialized");

        if (persistedToVolatile.ContainsKey(persistedEntityId))
            return persistedToVolatile[persistedEntityId];
        return persistedEntityId;
    }

    /// <summary>
    ///     Unloads the given entity from the mapper.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void UnloadId(ulong entityId)
    {
        if (!initialized) throw new InvalidOperationException("Not initialized");

        /* Remove all mappings in both directions. */
        if (persistedToVolatile.ContainsKey(entityId))
        {
            var volatileId = persistedToVolatile[entityId];
            persistedToVolatile.Remove(entityId);
            if (volatileToPersisted.ContainsKey(volatileId)) volatileToPersisted.Remove(volatileId);
        }

        if (volatileToPersisted.ContainsKey(entityId))
        {
            var persistedId = volatileToPersisted[entityId];
            volatileToPersisted.Remove(entityId);
            if (persistedToVolatile.ContainsKey(persistedId)) persistedToVolatile.Remove(persistedId);
        }
    }

    private ulong GetNewPersistedId(ulong volatileEntityId)
    {
        var persistedEntityId = NextPersistedId++;
        volatileToPersisted[volatileEntityId] = persistedEntityId;
        persistedToVolatile[persistedEntityId] = volatileEntityId;

        return persistedEntityId;
    }
}