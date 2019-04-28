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

using Castle.Core.Logging;
using Sovereign.EngineCore.Components;
using Sovereign.EngineCore.Main;
using Sovereign.Persistence.Database;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Sovereign.Persistence.State
{

    /// <summary>
    /// Manages changes to state of all persisted entities.
    /// </summary>
    public sealed class StateManager
    {
        private readonly ComponentManager componentManager;
        private readonly PersistenceProviderManager persistenceProviderManager;

        /// <summary>
        /// Front state buffer, currently accepting data.
        /// </summary>
        public StateBuffer FrontBuffer { get; private set; }

        /// <summary>
        /// Back state buffer, currently being processed or pending flip.
        /// </summary>
        private StateBuffer backBuffer;

        public StateManager(ComponentManager componentManager,
            PersistenceProviderManager persistenceProviderManager,
            ILogger logger, FatalErrorHandler fatalErrorHandler)
        {
            FrontBuffer = new StateBuffer(logger, fatalErrorHandler);
            backBuffer = new StateBuffer(logger, fatalErrorHandler);

            this.componentManager = componentManager;
            this.persistenceProviderManager = persistenceProviderManager;
        }

        /// <summary>
        /// Commits the changes in the front buffer, swapping the buffers
        /// in the process.
        /// </summary>
        public void CommitChanges()
        {
            /* Swap the buffers. */
            SwapBuffers();

            /* Back buffer now contains the pending updates; synchronize. */
            backBuffer.Synchronize(persistenceProviderManager.PersistenceProvider);
        }

        /// <summary>
        /// Atomically swaps the component state buffers.
        /// </summary>
        private void SwapBuffers()
        {
            /* Acquire strong lock to prevent component updates during swap. */
            using (var strongLock = componentManager.ComponentGuard.AcquireStrongLock())
            {
                /* Swap the buffers. */
                var swap = backBuffer;
                backBuffer = FrontBuffer;
                FrontBuffer = swap;

                /* Clear front buffer so that it can be reused. */
                FrontBuffer.Reset();
            }
        }

    }

}
