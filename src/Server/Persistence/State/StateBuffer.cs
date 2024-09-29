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
 * GNU General Public License for more detailsStateTracker.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Data;
using Castle.Core.Logging;
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;
using Sovereign.EngineUtil.Collections;
using Sovereign.Persistence.Database;
using Sovereign.Persistence.Database.Queries;
using Sovereign.Persistence.Systems.Persistence;

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

    /// <summary>
    ///     Admin state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<bool>> adminUpdates = new(BufferSize);

    /// <summary>
    ///     Animated sprite state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<int>> animatedSpriteUpdates = new(BufferSize);

    /// <summary>
    ///     CastBlockShadows state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<bool>> castBlockShadowsUpdates = new(BufferSize);

    /// <summary>
    ///     Drawable state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<bool>> drawableUpdates = new(BufferSize);

    private readonly IEventSender eventSender;
    private readonly FatalErrorHandler fatalErrorHandler;
    private readonly PersistenceInternalController internalController;
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
    ///     Orientation state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<Orientation>> orientationUpdates = new(BufferSize);

    /// <summary>
    ///     Parent entity linkage updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<ulong>> parentUpdates = new(BufferSize);

    /// <summary>
    ///     Player character tag state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<bool>> playerCharacterUpdates = new(BufferSize);

    /// <summary>
    ///     Position state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<Kinematics>> positionUpdates = new(BufferSize);

    /// <summary>
    ///     Removed entity IDs.
    /// </summary>
    private readonly StructBuffer<ulong> removedEntities = new(BufferSize);

    /// <summary>
    ///     Template state updates.
    /// </summary>
    private readonly StructBuffer<StateUpdate<ulong>> templateUpdates = new(BufferSize);

    private readonly WorldSegmentPersister worldSegmentPersister;

    public StateBuffer(ILogger logger, FatalErrorHandler fatalErrorHandler, IEventSender eventSender,
        PersistenceInternalController internalController, WorldSegmentPersister worldSegmentPersister)
    {
        this.logger = logger;
        this.fatalErrorHandler = fatalErrorHandler;
        this.eventSender = eventSender;
        this.internalController = internalController;
        this.worldSegmentPersister = worldSegmentPersister;
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
    ///     Queues a template update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateTemplate(ref StateUpdate<ulong> update)
    {
        templateUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a position update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdatePosition(ref StateUpdate<Kinematics> update)
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
    /// <param name="update">State update.</param>
    public void UpdatePlayerCharacter(ref StateUpdate<bool> update)
    {
        playerCharacterUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a name update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateName(ref StateUpdate<string> update)
    {
        nameUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues an account linkage update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateAccount(ref StateUpdate<Guid> update)
    {
        accountUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a parent entity linkage update.
    /// </summary>
    /// <param name="update">State update.</param>
    public void UpdateParent(ref StateUpdate<ulong> update)
    {
        parentUpdates.Add(ref update);
    }

    /// <summary>
    ///     Queues a drawable tag update.
    /// </summary>
    /// <param name="update"></param>
    public void UpdateDrawable(ref StateUpdate<bool> update)
    {
        drawableUpdates.Add(ref update);
    }

    /// <summary>
    ///     Enqueues an animated sprite update.
    /// </summary>
    /// <param name="update"></param>
    public void UpdateAnimatedSprite(ref StateUpdate<int> update)
    {
        animatedSpriteUpdates.Add(ref update);
    }

    /// <summary>
    ///     Enqueues an orientation update.
    /// </summary>
    /// <param name="update">Update.</param>
    public void UpdateOrientation(ref StateUpdate<Orientation> update)
    {
        orientationUpdates.Add(ref update);
    }

    /// <summary>
    ///     Enqueues an admin tag update.
    /// </summary>
    /// <param name="update">Update.</param>
    public void UpdateAdmin(ref StateUpdate<bool> update)
    {
        adminUpdates.Add(ref update);
    }

    public void UpdateCastBlockShadows(ref StateUpdate<bool> update)
    {
        castBlockShadowsUpdates.Add(ref update);
    }

    /// <summary>
    ///     Resets the buffer.
    /// </summary>
    public void Reset()
    {
        newEntities.Clear();
        removedEntities.Clear();
        templateUpdates.Clear();
        positionUpdates.Clear();
        materialUpdates.Clear();
        materialModifierUpdates.Clear();
        playerCharacterUpdates.Clear();
        nameUpdates.Clear();
        accountUpdates.Clear();
        parentUpdates.Clear();
        drawableUpdates.Clear();
        animatedSpriteUpdates.Clear();
        orientationUpdates.Clear();
        adminUpdates.Clear();
        castBlockShadowsUpdates.Clear();
    }

    /// <summary>
    ///     Synchronizes the buffer with the database.
    /// </summary>
    /// <param name="persistenceProvider">Persistence provider.</param>
    public void Synchronize(IPersistenceProvider persistenceProvider)
    {
        DoSynchronize(persistenceProvider);
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
                // Synchronize the entities first before the components.
                // This ensures that any foreign key relationships between components
                // and entities are satisfied when the components are updated.
                SynchronizeAddedEntities(persistenceProvider, transaction);

                // Next process any pending updates to entity templates.
                SynchronizeTemplates(persistenceProvider.SetTemplateQuery, transaction);

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

                /* Parent. */
                SynchronizeComponent(parentUpdates,
                    persistenceProvider.AddParentComponentQuery,
                    persistenceProvider.ModifyParentComponentQuery,
                    persistenceProvider.RemoveParentComponentQuery,
                    transaction);

                /* Drawable. */
                SynchronizeComponent(drawableUpdates,
                    persistenceProvider.AddDrawableComponentQuery,
                    persistenceProvider.ModifyDrawableComponentQuery,
                    persistenceProvider.RemoveDrawableComponentQuery,
                    transaction);

                /* AnimatedSprite. */
                SynchronizeComponent(animatedSpriteUpdates,
                    persistenceProvider.AddAnimatedSpriteComponentQuery,
                    persistenceProvider.ModifyAnimatedSpriteComponentQuery,
                    persistenceProvider.RemoveAnimatedSpriteComponentQuery,
                    transaction);

                // Orientation.
                SynchronizeComponent(orientationUpdates,
                    persistenceProvider.AddOrientationComponentQuery,
                    persistenceProvider.ModifyOrientationComponentQuery,
                    persistenceProvider.RemoveOrientationComponentQuery,
                    transaction);

                // Admin.
                SynchronizeComponent(adminUpdates,
                    persistenceProvider.AddAdminComponentQuery,
                    persistenceProvider.ModifyAdminComponentQuery,
                    persistenceProvider.RemoveAdminComponentQuery,
                    transaction);

                // CastBlockShadows.
                SynchronizeComponent(castBlockShadowsUpdates,
                    persistenceProvider.AddCastBlockShadowsComponentQuery,
                    persistenceProvider.ModifyCastBlockShadowsComponentQuery,
                    persistenceProvider.RemoveCastBlockShadowsComponentQuery,
                    transaction);

                SynchronizeRemovedEntities(persistenceProvider, transaction);

                worldSegmentPersister.SynchronizeWorldSegments(persistenceProvider, transaction);

                transaction.Commit();
            }

            internalController.CompleteSync(eventSender);
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
    ///     Synchronizes pending template updates.
    /// </summary>
    /// <param name="setTemplateQuery">Set template query.</param>
    /// <param name="transaction">Transaction.</param>
    private void SynchronizeTemplates(ISetTemplateQuery setTemplateQuery, IDbTransaction transaction)
    {
        for (var i = 0; i < templateUpdates.Count; ++i)
        {
            ref var update = ref templateUpdates[i];
            setTemplateQuery.SetTemplate(update.EntityId, update.Value, transaction);
        }
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