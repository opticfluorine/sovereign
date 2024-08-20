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

namespace Sovereign.EngineUtil.Threading;

/// <summary>
///     Synchronization primitive that supports both mutual exclusion and
///     a reverse semaphore-like behavior.
/// </summary>
public sealed class IncrementalGuard
{
    /// <summary>
    ///     Object used for internal locking.
    /// </summary>
    private readonly object innerLock = new();

    /// <summary>
    ///     Number of active weak locks.
    /// </summary>
    private int weakLockCount;

    /// <summary>
    ///     Acquires a weak lock.
    /// </summary>
    /// <returns>Disposable handle to the weak lock.</returns>
    /// <remarks>
    ///     More than one thread may hold a valid weak lock at the same time.
    /// </remarks>
    public IncrementalGuardWeakLock AcquireWeakLock()
    {
        return new IncrementalGuardWeakLock(this);
    }

    /// <summary>
    ///     Acquires a strong lock.
    /// </summary>
    /// <returns>Disposable handle to the strong lock.</returns>
    /// <remarks>
    ///     A strong lock may only be held by a single thread at once, and it may
    ///     only be held when no weak locks are held.
    /// </remarks>
    public IncrementalGuardStrongLock AcquireStrongLock()
    {
        return new IncrementalGuardStrongLock(this);
    }

    /// <summary>
    ///     Takes a weak lock.
    /// </summary>
    private void TakeWeakLock()
    {
        lock (innerLock)
        {
            Interlocked.Increment(ref weakLockCount);
        }
    }

    /// <summary>
    ///     Releases a weak lock.
    /// </summary>
    private void ReleaseWeakLock()
    {
        Interlocked.Decrement(ref weakLockCount);
    }

    /// <summary>
    ///     Takes the strong lock.
    /// </summary>
    private void TakeStrongLock()
    {
        Monitor.Enter(innerLock);
        SpinWait.SpinUntil(CanTakeStrongLock);
    }

    /// <summary>
    ///     Checks whether the strong lock can be taken.
    /// </summary>
    /// <returns>true if it can be taken, false otherwise.</returns>
    private bool CanTakeStrongLock()
    {
        return weakLockCount == 0;
    }

    /// <summary>
    ///     Releases the strong lock.
    /// </summary>
    private void ReleaseStrongLock()
    {
        Monitor.Exit(innerLock);
    }

    /// <summary>
    ///     Disposable handle to the weak locking mode.
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
    ///     Disposable handle to the strong locking mode.
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