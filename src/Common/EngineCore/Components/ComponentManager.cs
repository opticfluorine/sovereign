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

using System.Collections.Generic;
using Sovereign.EngineCore.Entities;
using Sovereign.EngineUtil.Threading;

namespace Sovereign.EngineCore.Components;

/// <summary>
///     Manages updates to all components.
/// </summary>
public class ComponentManager
{
    /// <summary>
    ///     All known component removers.
    /// </summary>
    private readonly IList<IComponentRemover> componentRemovers = new List<IComponentRemover>();

    /// <summary>
    ///     All known component updaters.
    /// </summary>
    private readonly IList<IComponentUpdater> componentUpdaters = new List<IComponentUpdater>();

    private readonly EntityNotifier entityNotifier;

    public ComponentManager(EntityNotifier entityNotifier)
    {
        this.entityNotifier = entityNotifier;
    }

    /// <summary>
    ///     Incremental guard used to synchronize component updates.
    /// </summary>
    public IncrementalGuard ComponentGuard { get; } = new();

    /// <summary>
    ///     Registers a component updater with the manager.
    /// </summary>
    /// <param name="updater">Component updater to be registered.</param>
    public void RegisterComponentUpdater(IComponentUpdater updater)
    {
        componentUpdaters.Add(updater);
    }

    /// <summary>
    ///     Applies all pending changes to all components.
    ///     This method should only be called from the main thread.
    /// </summary>
    public void UpdateAllComponents()
    {
        using (var strongLock = ComponentGuard.AcquireStrongLock())
        {
            /* Update components. */
            foreach (var updater in componentUpdaters) updater.ApplyComponentUpdates();

            /* Trigger pending entity events. */
            entityNotifier.Dispatch();
        }
    }

    /// <summary>
    ///     Removes all components for the given entity.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void RemoveAllComponentsForEntity(ulong entityId)
    {
        foreach (var remover in componentRemovers) remover.RemoveComponent(entityId);
    }

    /// <summary>
    ///     Unloads all components for the given entity, but does not
    ///     formally delete them.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    public void UnloadAllComponentsForEntity(ulong entityId)
    {
    }

    /// <summary>
    ///     Register a component remover.
    /// </summary>
    /// <param name="componentRemover">Component remover.</param>
    public void RegisterComponentRemover(IComponentRemover componentRemover)
    {
        componentRemovers.Add(componentRemover);
    }
}