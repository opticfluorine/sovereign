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

using System.Collections.Generic;

namespace Sovereign.EngineCore.Entities;

/// <summary>
///     Responsible for notifying listeners of entity updates.
/// </summary>
public sealed class EntityNotifier
{
    /// <summary>
    ///     Remove events pending dispatch.
    /// </summary>
    private readonly ISet<ulong> queuedRemoveEvents = new HashSet<ulong>();

    /// <summary>
    ///     Unload events pending dispatch.
    /// </summary>
    private readonly ISet<ulong> queuedUnloadEvents = new HashSet<ulong>();

    /// <summary>
    ///     Event triggered when an entity is removed.
    /// </summary>
    public event EntityEventDelegates.RemoveEntityEvent OnRemoveEntity;

    /// <summary>
    ///     Event triggered when an entity is unloaded.
    /// </summary>
    public event EntityEventDelegates.UnloadEntityEvent OnUnloadEntity;

    /// <summary>
    ///     Enqueues an entity remove event.
    /// </summary>
    /// <param name="entityId">Removed entity ID.</param>
    public void EnqueueRemove(ulong entityId)
    {
        queuedRemoveEvents.Add(entityId);
    }

    /// <summary>
    ///     Enqueues an entity unload event.
    /// </summary>
    /// <param name="entityId">Unloaded entity ID.</param>
    public void EnqueueUnload(ulong entityId)
    {
        queuedUnloadEvents.Add(entityId);
    }

    /// <summary>
    ///     Dispatches all entity events.
    /// </summary>
    /// <remarks>
    ///     This should only be called while the strong update lock is held
    ///     immediately following component updates.
    /// </remarks>
    public void Dispatch()
    {
        foreach (var entityId in queuedRemoveEvents) OnRemoveEntity.Invoke(entityId);
        foreach (var entityId in queuedUnloadEvents) OnUnloadEntity.Invoke(entityId);

        queuedRemoveEvents.Clear();
        queuedUnloadEvents.Clear();
    }
}