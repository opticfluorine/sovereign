/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.EngineCore.Entities;
using Sovereign.Persistence.Database.Queries;
using System;
using System.Collections.Generic;
using System.Data;

namespace Sovereign.Persistence.Entities
{

    /// <summary>
    /// Maps persisted entity IDs to volatile entity IDs and vice versa.
    /// </summary>
    public sealed class EntityMapper
    {

        private IDictionary<ulong, ulong> volatileToPersisted
            = new Dictionary<ulong, ulong>();

        private IDictionary<ulong, ulong> persistedToVolatile
            = new Dictionary<ulong, ulong>();

        private INextPersistedIdQuery nextIdQuery;

        /// <summary>
        /// Whether the subsystem has been initialized.
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Next available persisted ID.
        /// </summary>
        public ulong NextPersistedId { get; private set; }

        /// <summary>
        /// Initializes the mapper to generate unique persisted IDs.
        /// </summary>
        public void InitializeMapper(INextPersistedIdQuery nextIdQuery)
        {
            this.nextIdQuery = nextIdQuery;
            NextPersistedId = nextIdQuery.GetNextPersistedEntityId();

            initialized = true;
        }

        /// <summary>
        /// Gets the persisted entity ID for the given volatile entity ID.
        /// </summary>
        /// <param name="volatileEntityId">Volatile entity ID.</param>
        /// <param name="needToCreate">Set to true if the entity ID needs to
        /// be added to the database.</param>
        /// <returns>Persisted entity ID.</returns>
        public ulong GetPersistedId(ulong volatileEntityId, out bool needToCreate)
        {
            if (!initialized) throw new InvalidOperationException("Not initialized");

            needToCreate = false;
            if (volatileEntityId >= EntityAssigner.FirstPersistedId)
            {
                return volatileEntityId;
            }
            else if (volatileToPersisted.ContainsKey(volatileEntityId))
            {
                return volatileToPersisted[volatileEntityId];
            }
            else
            {
                needToCreate = true;
                return GetNewPersistedId(volatileEntityId);
            }
        }

        /// <summary>
        /// Gets the volatile entity ID for the given persisted entity ID.
        /// </summary>
        /// <param name="persistedEntityId">Persisted entity ID.</param>
        /// <returns>Volatile entity ID.</returns>
        /// <remarks>
        /// If no volatile entity ID is known for the persisted ID, this method
        /// returns the persisted ID.
        /// </remarks>
        public ulong GetVolatileId(ulong persistedEntityId)
        {
            if (!initialized) throw new InvalidOperationException("Not initialized");

            if (persistedToVolatile.ContainsKey(persistedEntityId))
                return persistedToVolatile[persistedEntityId];
            else
                return persistedEntityId;
        }

        private ulong GetNewPersistedId(ulong volatileEntityId)
        {
            var persistedEntityId = NextPersistedId++;
            volatileToPersisted[volatileEntityId] = persistedEntityId;
            persistedToVolatile[persistedEntityId] = volatileEntityId;

            return persistedEntityId;
        }

    }

}
