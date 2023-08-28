﻿/*
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

using System;
using System.Data;
using System.Numerics;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Sovereign.EngineCore.Main;
using Sovereign.EngineUtil.Collections;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;

namespace Sovereign.Persistence.State;

/// <summary>
///     State update buffer.
/// </summary>
public sealed class StateBuffer
{
    /// <summary>
    ///     Initial size of each state update buffer.
    /// </summary>
    private const int BufferSize = 8192;

    /// <summary>
    ///     Account state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<Guid>> accountUpdates = new(BufferSize);

    private readonly FatalErrorHandler fatalErrorHandler;

    private readonly ILogger logger;

    /// <summary>
    ///     Material modifier state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<int>> materialModifierUpdates = new(BufferSize);

    /// <summary>
    ///     Material state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<int>> materialUpdates = new(BufferSize);

    /// <summary>
    ///     Name component updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<string>> nameUpdates = new(BufferSize);

    /// <summary>
    ///     New entity IDs.
    /// </summary>
    private readonly StructBuffer<ulong> newEntities = new(BufferSize);

    /// <summary>
    ///     Player character tag state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<bool>> playerCharacterUpdates = new(BufferSize);

    /// <summary>
    ///     Position state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<Vector3>> positionUpdates = new(BufferSize);

    /// <summary>
    ///     Removed entity IDs.
    /// </summary>
    private readonly StructBuffer<ulong> removedEntities = new(BufferSize);

    public StateBuffer(ILogger logger, FatalErrorHandler fatalErrorHandler)
    {
        this.logger = logger;
        this.fatalErrorHandler = fatalErrorHandler;
    }

    /// <summary>
    ///     Queues an entity for database insert.
    /// </summary>
    /// <param name="entityId">Persisted entity ID.</param>
    public void AddEntity(ulong entityId)
    {
        newEntities.Add(ref entityId);
    }

    /// <summary>
    ///     Queues an entity for database removal.
    /// </summary>
    /// <param name="entityId">Persisted entity ID.</param>
    public void RemoveEntity(ulong entityId)
    {
        removedEntities.Add(ref entityId);
    }

    /// <summary>
    ///     Queues a position update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdatePosition(ref StateUpdate<Vector3> update)
    {
        positionUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a material update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateMaterial(ref StateUpdate<int> update)
    {
        materialUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a material modifier update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateMaterialModifier(ref StateUpdate<int> update)
    {
        materialModifierUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a player character update.
    /// </summary>
    /// <param name="update"></param>
    public void UpdatePlayerCharacter(ref StateUpdate<bool> update)
    {
        playerCharacterUpdates.Add(ref update);
    }

    public void UpdateName(ref StateUpdate<string> update)
    {
        nameUpdates.Add(ref update);
    }

    public void UpdateAccount(ref StateUpdate<Guid> update)
    {
        accountUpdates.Add(ref update);
    }

    /// <summary>
    ///     Resets the buffer.
    /// </summary>
    public void Reset()
    {
        newEntities.Clear();
        removedEntities.Clear();
        positionUpdates.Clear();
        materialUpdates.Clear();
        materialModifierUpdates.Clear();
        playerCharacterUpdates.Clear();
        nameUpdates.Clear();
        accountUpdates.Clear();
    }

    /// <summary>
    ///     Synchronizes the buffer with the database.
    /// </summary>
    /// <param name="persistenceProvider">Persistence provider.</param>
    public void Synchronize(IPersistenceProvider persistenceProvider)
    {
        Task.Run(() => DoSynchronize(persistenceProvider));
    }

    /// <summary>
    ///     Actually performs the synchronization.
    /// </summary>
    /// <param name="persistenceProvider">Persistence provider.</param>
    private void DoSynchronize(IPersistenceProvider persistenceProvider)
    {
        try
        {
            using (var transaction = persistenceProvider.Connection.BeginTransaction())
            {
                SynchronizeAddedEntities(persistenceProvider, transaction);

                /* Position. */
                SynchronizeComponent(positionUpdates,
                    persistenceProvider.AddPositionQuery,
                    persistenceProvider.ModifyPositionQuery,
                    persistenceProvider.RemovePositionQuery,
                    transaction);

                /* Material. */
                SynchronizeComponent(materialUpdates,
                    persistenceProvider.AddMaterialQuery,
                    persistenceProvider.ModifyMaterialQuery,
                    persistenceProvider.RemoveMaterialQuery,
                    transaction);

                /* MaterialModifier. */
                SynchronizeComponent(materialModifierUpdates,
                    persistenceProvider.AddMaterialModifierQuery,
                    persistenceProvider.ModifyMaterialModifierQuery,
                    persistenceProvider.RemoveMaterialModifierQuery,
                    transaction);

                /* PlayerCharacter. */
                SynchronizeComponent(playerCharacterUpdates,
                    persistenceProvider.AddPlayerCharacterQuery,
                    persistenceProvider.ModifyPlayerCharacterQuery,
                    persistenceProvider.RemovePlayerCharacterQuery,
                    transaction);

                /* Name. */
                SynchronizeComponent(nameUpdates,
                    persistenceProvider.AddNameQuery,
                    persistenceProvider.ModifyNameQuery,
                    persistenceProvider.RemoveNameQuery,
                    transaction);

                /* Account. */
                SynchronizeComponent(accountUpdates,
                    persistenceProvider.AddAccountComponentQuery,
                    persistenceProvider.ModifyAccountComponentQuery,
                    persistenceProvider.RemoveAccountComponentQuery,
                    transaction);

                SynchronizeRemovedEntities(persistenceProvider, transaction);

                transaction.Commit();
            }
        }
        catch (Exception e)
        {
            logger.Fatal("Error while synchronizing database.", e);
            fatalErrorHandler.FatalError();
        }
    }

    /// <summary>
    ///     Adds new entities to the database.
    /// </summary>
    /// <param name="persistenceProvider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    private void SynchronizeAddedEntities(IPersistenceProvider persistenceProvider,
        IDbTransaction transaction)
    {
        var query = persistenceProvider.AddEntityQuery;
        foreach (var entityId in newEntities) query.AddEntity(entityId, transaction);
    }

    /// <summary>
    ///     Removes entities from the database.
    /// </summary>
    /// <param name="persistenceProvider">Persistence provider.</param>
    /// <param name="transaction">Transaction.</param>
    private void SynchronizeRemovedEntities(IPersistenceProvider persistenceProvider,
        IDbTransaction transaction)
    {
        var query = persistenceProvider.RemoveEntityQuery;
        foreach (var entityId in removedEntities) query.RemoveEntityId(entityId, transaction);
    }

    /// <summary>
    ///     Synchronizes a component buffer.
    /// </summary>
    /// <typeparam name="T">Component type.</typeparam>
    /// <param name="buffer">Update buffer.</param>
    /// <param name="addQuery">Add query.</param>
    /// <param name="modQuery">Modify query.</param>
    /// <param name="remQuery">Remove query.</param>
    /// <param name="transaction">Transaction.</param>
    private void SynchronizeComponent<T>(StructBuffer<StateUpdate<T>> buffer,
        IAddComponentQuery<T> addQuery, IModifyComponentQuery<T> modQuery,
        IRemoveComponentQuery remQuery, IDbTransaction transaction)
    {
        foreach (var update in buffer)
            switch (update.StateUpdateType)
            {
                case StateUpdateType.Add:
                    addQuery.Add(update.EntityId, update.Value, transaction);
                    break;

                case StateUpdateType.Modify:
                    modQuery.Modify(update.EntityId, update.Value, transaction);
                    break;

                case StateUpdateType.Remove:
                    remQuery.Remove(update.EntityId, transaction);
                    break;
            }
    }
}