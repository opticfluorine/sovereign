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

using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.EngineCore.Entities
{

    /// <summary>
    /// Responsible for notifying listeners of entity updates.
    /// </summary>
    public sealed class EntityNotifier
    {

        /// <summary>
        /// Event triggered when an entity is removed.
        /// </summary>
        public event EntityEventDelegates.RemoveEntityEvent OnRemoveEntity;
        
        /// <summary>
        /// Event triggered when an entity is unloaded.
        /// </summary>
        public event EntityEventDelegates.UnloadEntityEvent OnUnloadEntity;

        /// <summary>
        /// Remove events pending dispatch.
        /// </summary>
        private ISet<ulong> queuedRemoveEvents = new HashSet<ulong>();

        /// <summary>
        /// Unload events pending dispatch.
        /// </summary>
        private ISet<ulong> queuedUnloadEvents = new HashSet<ulong>();

        /// <summary>
        /// Enqueues an entity remove event.
        /// </summary>
        /// <param name="entityId">Removed entity ID.</param>
        public void EnqueueRemove(ulong entityId)
        {
            queuedRemoveEvents.Add(entityId);
        }

        /// <summary>
        /// Enqueues an entity unload event.
        /// </summary>
        /// <param name="entityId">Unloaded entity ID.</param>
        public void EnqueueUnload(ulong entityId)
        {
            queuedUnloadEvents.Add(entityId);
        }

        /// <summary>
        /// Dispatches all entity events.
        /// </summary>
        /// <remarks>
        /// This should only be called while the strong update lock is held
        /// immediately following component updates.
        /// </remarks>
        public void Dispatch()
        {
            foreach (var entityId in queuedRemoveEvents)
            {
                OnRemoveEntity.Invoke(entityId);
            }
            foreach (var entityId in queuedUnloadEvents)
            {
                OnUnloadEntity.Invoke(entityId);
            }

            queuedRemoveEvents.Clear();
            queuedUnloadEvents.Clear();
        }

    }

}
