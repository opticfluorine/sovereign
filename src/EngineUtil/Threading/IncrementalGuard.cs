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

using System;
using System.Threading;

namespace Sovereign.EngineUtil.Threading
{

    /// <summary>
    /// Synchronization primitive that supports both mutual exclusion and
    /// a reverse semaphore-like behavior.
    /// </summary>
    public sealed class IncrementalGuard
    {

        /// <summary>
        /// Object used for internal locking.
        /// </summary>
        private readonly object innerLock = new object();

        /// <summary>
        /// Number of active weak locks.
        /// </summary>
        private int weakLockCount = 0;

        /// <summary>
        /// Acquires a weak lock.
        /// </summary>
        /// <returns>Disposable handle to the weak lock.</returns>
        /// <remarks>
        /// More than one thread may hold a valid weak lock at the same time.
        /// </remarks>
        public IncrementalGuardWeakLock AcquireWeakLock()
        {
            return new IncrementalGuardWeakLock(this);
        }

        /// <summary>
        /// Acquires a strong lock.
        /// </summary>
        /// <returns>Disposable handle to the strong lock.</returns>
        /// <remarks>
        /// A strong lock may only be held by a single thread at once, and it may
        /// only be held when no weak locks are held.
        /// </remarks>
        public IncrementalGuardStrongLock AcquireStrongLock()
        {
            return new IncrementalGuardStrongLock(this);
        }

        /// <summary>
        /// Takes a weak lock.
        /// </summary>
        private void TakeWeakLock()
        {
            lock (innerLock)
            {
                Interlocked.Increment(ref weakLockCount);
            }
        }

        /// <summary>
        /// Releases a weak lock.
        /// </summary>
        private void ReleaseWeakLock()
        {
            Interlocked.Decrement(ref weakLockCount);
        }

        /// <summary>
        /// Takes the strong lock.
        /// </summary>
        private void TakeStrongLock()
        {
            Monitor.Enter(innerLock);
            SpinWait.SpinUntil(() => weakLockCount == 0);
        }

        /// <summary>
        /// Releases the strong lock.
        /// </summary>
        private void ReleaseStrongLock()
        {
            Monitor.Exit(innerLock);
        }

        /// <summary>
        /// Disposable handle to the weak locking mode.
        /// </summary>
        public sealed class IncrementalGuardWeakLock : IDisposable
        {
            private readonly IncrementalGuard guard;

            internal IncrementalGuardWeakLock(IncrementalGuard guard)
            {
                this.guard = guard;

                guard.TakeWeakLock();
            }

            public void Dispose()
            {
                guard.ReleaseWeakLock();
            }
        }

        /// <summary>
        /// Disposable handle to the strong locking mode.
        /// </summary>
        public sealed class IncrementalGuardStrongLock : IDisposable
        {
            private readonly IncrementalGuard guard;

            internal IncrementalGuardStrongLock(IncrementalGuard guard)
            {
                this.guard = guard;

                guard.TakeStrongLock();
            }

            public void Dispose()
            {
                guard.ReleaseStrongLock();
            }
        }

    }

}

