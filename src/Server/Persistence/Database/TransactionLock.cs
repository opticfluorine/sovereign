// Sovereign Engine
// Copyright (c) 2025 opticfluorine
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Threading;

namespace Sovereign.Persistence.Database;

/// <summary>
///     Provides a locking mechanism to ensure that there are no concurrent transactions
///     with database providers that do not support concurrent writes.
/// </summary>
public interface ITransactionLock
{
    /// <summary>
    ///     Acquires the transaction lock, blocking until the lock can be acquired.
    /// </summary>
    void Acquire();

    /// <summary>
    ///     Releases the transaction lock.
    /// </summary>
    void Release();
}

/// <summary>
///     Locking implementation of ITransactionLock.
/// </summary>
public class SingleWriterTransactionLock : ITransactionLock
{
    private readonly Lock tLock = new();

    public void Acquire()
    {
        tLock.Enter();
    }

    public void Release()
    {
        tLock.Exit();
    }
}

/// <summary>
///     Non-locking implementation of ITransactionLock.
/// </summary>
public class NullTransactionLock : ITransactionLock
{
    public void Acquire()
    {
    }

    public void Release()
    {
    }
}