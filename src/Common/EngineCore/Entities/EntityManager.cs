/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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
using System.Threading;
using Sovereign.EngineCore.Components;
using Sovereign.EngineUtil.Threading;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Top-level manager of the entity infrastructure.
/// </summary>
/// The methods exposed by this class are thread-safe.
public class EntityManager
{
    /// <summary>
    ///     First block reserved for use by automatic assignment.
    /// </summary>
    public const uint FirstReservedBlock = 0;

    /// <summary>
    ///     First block not reserved for use by automatic assignment.
    /// </summary>
    public const uint FirstUnreservedBlock = 16777216;

    private readonly ComponentManager componentManager;
    private readonly EntityNotifier entityNotifier;

    private readonly EntityTable entityTable;

    /// <summary>
    ///     Next block identifier.
    /// </summary>
    private int nextBlock = (int)FirstReservedBlock;

    public EntityManager(ComponentManager componentManager,
        EntityNotifier entityNotifier,
        EntityTable entityTable)
    {
        this.componentManager = componentManager;
        this.entityNotifier = entityNotifier;
        this.entityTable = entityTable;
    }

    /// <summary>
    ///     Incremental guard used to synchronize component updates.
    /// </summary>
    public IncrementalGuard UpdateGuard { get; } = new();

    /// <summary>
    ///     Applies all entity and component updates.
    /// </summary>
    public void ApplyUpdates()
    {
        using var strongLock = UpdateGuard.AcquireStrongLock();

        componentManager.UpdateAllComponents();
        entityTable.UpdateAllEntities();
        OnUpdatesComplete?.Invoke();
    }

    /// <summary>
    ///     Gets a new EntityAssigner.
    /// </summary>
    /// This method is thread-safe.
    /// <returns>New EntityAssigner over the next available block.</returns>
    public EntityAssigner GetNewAssigner()
    {
        var block = Interlocked.Increment(ref nextBlock);
        return GetNewAssigner((uint)block);
    }

    /// <summary>
    ///     Gets a new EntityAssigner for the given block.
    /// </summary>
    /// No checking of whether the block is already in use is performed. If
    /// the block is already in use, this may lead to entity ID collisions.
    /// 
    /// This method is thread-safe.
    /// <param name="block">Block identifier.</param>
    /// <returns>New EntityAssigner over the given block.</returns>
    public EntityAssigner GetNewAssigner(uint block)
    {
        return new EntityAssigner(block);
    }

    /// <summary>
    ///     Removes the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID to be removed.</param>
    public void RemoveEntity(ulong entityId)
    {
        componentManager.RemoveAllComponentsForEntity(entityId);
        entityNotifier.EnqueueRemove(entityId);
    }

    /// <summary>
    ///     Unloads the given entity, but does not formally delete it.
    /// </summary>
    /// <param name="entityId">Entity ID to be unloaded.</param>
    public void UnloadEntity(ulong entityId)
    {
        componentManager.UnloadAllComponentsForEntity(entityId);
        entityNotifier.EnqueueUnload(entityId);
    }

    /// <summary>
    ///     Event triggered when a round of entity/component updates are complete.
    /// </summary>
    public event Action? OnUpdatesComplete;
}