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
    private readonly HashSet<ulong> queuedRemoveEvents = new();

    /// <summary>
    ///     Unload events pending dispatch.
    /// </summary>
    private readonly HashSet<ulong> queuedUnloadEvents = new();

    /// <summary>
    ///     Event triggered when an entity is removed.
    /// </summary>
    public event EntityEventDelegates.RemoveEntityEvent? OnRemoveEntity;

    /// <summary>
    ///     Event triggered when an entity is unloaded.
    /// </summary>
    public event EntityEventDelegates.UnloadEntityEvent? OnUnloadEntity;

    /// <summary>
    ///     Enqueues an entity remove event.
    /// </summary>
    /// <param name="entityId">Removed entity ID.</param>
    public void EnqueueRemove(ulong entityId)
    {
        lock (queuedRemoveEvents)
        {
            queuedRemoveEvents.Add(entityId);
        }
    }

    /// <summary>
    ///     Enqueues an entity unload event.
    /// </summary>
    /// <param name="entityId">Unloaded entity ID.</param>
    public void EnqueueUnload(ulong entityId)
    {
        lock (queuedUnloadEvents)
        {
            queuedUnloadEvents.Add(entityId);
        }
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
        lock (queuedRemoveEvents)
        {
            foreach (var entityId in queuedRemoveEvents) OnRemoveEntity?.Invoke(entityId);
            queuedRemoveEvents.Clear();
        }

        lock (queuedUnloadEvents)
        {
            foreach (var entityId in queuedUnloadEvents) OnUnloadEntity?.Invoke(entityId);
            queuedUnloadEvents.Clear();
        }
    }
}