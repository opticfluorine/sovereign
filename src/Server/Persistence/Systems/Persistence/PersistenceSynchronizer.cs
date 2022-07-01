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
using Sovereign.Persistence.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sovereign.Persistence.Systems.Persistence
{

    /// <summary>
    /// Responsible for synchronizing the database.
    /// </summary>
    public sealed class PersistenceSynchronizer
    {
        private readonly StateManager stateManager;

        public ILogger Logger { private get; set; } = NullLogger.Instance;

        public PersistenceSynchronizer(StateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        /// <summary>
        /// Synchronizes the database.
        /// </summary>
        public void Synchronize()
        {
            Logger.Info("Synchronize database.");

            stateManager.CommitChanges();
        }

    }

}
