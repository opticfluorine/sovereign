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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Sovereign.EngineUtil.Collections;

/// <summary>
///     Maintains a reusable buffer of structs.
/// </summary>
/// This class is thread-safe. Note that generic enumerators for a StructBuffer lock
/// the buffer for their entire existence; the lock is only released when the
/// enumerator's Dispose() method is called. Note that this means that enumerators
/// cannot be shared between threads. If cross-threaded enumeration is needed, first
/// rethink your design, and if you still need it, use the old-style enumerator.
/// <typeparam name="T">Struct type. Must be mutable.</typeparam>
/// <inheritdoc />
public class StructBuffer<T> : IEnumerable<T>
{
    /// <summary>
    ///     Initial capacity.
    /// </summary>
    private readonly int initialCapacity;

    /// <summary>
    ///     Backing list.
    /// </summary>
    private T[] list;

    /// <summary>
    ///     Creates a buffer with the given number of objects.
    /// </summary>
    /// <param name="capacity">Capacity.</param>
    public StructBuffer(int capacity)
    {
        initialCapacity = capacity;
        list = new T[capacity];
    }

    /// <summary>
    ///     Buffer capacity.
    /// </summary>
    public int Capacity => list.Length;

    /// <summary>
    ///     Number of active objects in the buffer.
    /// </summary>
    public int Count { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        return new StructBufferEnumerator<T>(this);
    }

    /// <summary>
    ///     This operation is not supported - use the generic version instead.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Adds an item to the buffer.
    /// </summary>
    /// <param name="item">Adds the item to the buffer.</param>
    public void Add(ref T item)
    {
        lock (list)
        {
            if (Count == Capacity) ResizeList();
            list[Count++] = item;
        }
    }

    /// <summary>
    ///     Resets the buffer.
    /// </summary>
    public void Clear()
    {
        lock (list)
        {
            Count = 0;
        }
    }

    /// <summary>
    ///     Increases the size of the backing list.
    /// </summary>
    private void ResizeList()
    {
        var newList = new T[Capacity + initialCapacity];
        Array.Copy(list, newList, Capacity);
        list = newList;
    }

    public override string ToString()
    {
        return "Count = " + Count;
    }

    /// <summary>
    ///     Generic enumerator for StructBuffer.
    /// </summary>
    /// <typeparam name="T1">Element type of the underlying StructBuffer.</typeparam>
    /// <inheritdoc />
    public struct StructBufferEnumerator<T1> : IEnumerator<T1>
    {
        /// <summary>
        ///     Struct buffer associated with this enumerator.
        /// </summary>
        private readonly StructBuffer<T1> structBuffer;

        private T1? current;

        /// <summary>
        ///     Current index into the buffer.
        /// </summary>
        private int currentIndex;

        public StructBufferEnumerator(StructBuffer<T1> structBuffer)
        {
            this.structBuffer = structBuffer;
            currentIndex = -1;

            Monitor.Enter(structBuffer);
            current = default;
        }

        public T1 Current
        {
            get
            {
                if (current == null) throw new InvalidOperationException("Enumerator is before first element.");

                return current;
            }
            private set => current = value;
        }

        object IEnumerator.Current
        {
            get
            {
                if (current == null) throw new InvalidOperationException("Enumerator is before first element.");

                return current;
            }
        }

        public void Dispose()
        {
            Monitor.Exit(structBuffer);
        }

        public bool MoveNext()
        {
            currentIndex++;
            if (currentIndex >= structBuffer.Count) return false;
            Current = structBuffer.list[currentIndex];
            return true;
        }

        public void Reset()
        {
            currentIndex = 0;
        }
    }
}