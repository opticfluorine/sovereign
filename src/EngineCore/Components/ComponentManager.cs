/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using Sovereign.EngineUtil.Threading;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Manages updates to all components.
    /// </summary>
    public class ComponentManager
    {

        /// <summary>
        /// Incremental guard used to synchronize component updates.
        /// </summary>
        public IncrementalGuard ComponentGuard { get; } = new IncrementalGuard();

        /// <summary>
        /// All known component updaters.
        /// </summary>
        private readonly IList<IComponentUpdater> componentUpdaters = new List<IComponentUpdater>();

        /// <summary>
        /// All known component removers.
        /// </summary>
        private readonly IList<IComponentRemover> componentRemovers = new List<IComponentRemover>();

        /// <summary>
        /// Registers a component updater with the manager.
        /// </summary>
        /// <param name="updater">Component updater to be registered.</param>
        public void RegisterComponentUpdater(IComponentUpdater updater)
        {
            componentUpdaters.Add(updater);
        }

        /// <summary>
        /// Applies all pending changes to all components.
        /// 
        /// This method should only be called from the main thread.
        /// </summary>
        public void UpdateAllComponents()
        {
            using (var strongLock = ComponentGuard.AcquireStrongLock())
            {
                foreach (var updater in componentUpdaters)
                {
                    updater.ApplyComponentUpdates();
                }
            }
        }

        /// <summary>
        /// Removes all components for the given entity.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void RemoveAllComponentsForEntity(ulong entityId)
        {
            foreach (var remover in componentRemovers)
            {
                remover.RemoveComponent(entityId);
            }
        }

        /// <summary>
        /// Unloads all components for the given entity, but does not
        /// formally delete them.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public void UnloadAllComponentsForEntity(ulong entityId)
        {

        }

        /// <summary>
        /// Register a component remover.
        /// </summary>
        /// <param name="componentRemover">Component remover.</param>
        public void RegisterComponentRemover(IComponentRemover componentRemover)
        {
            componentRemovers.Add(componentRemover);
        }

    }

}
