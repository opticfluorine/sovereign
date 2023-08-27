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
        Monitor.Enter(list);
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
    public class StructBufferEnumerator<T1> : IEnumerator<T1>
    {
        /// <summary>
        ///     Struct buffer associated with this enumerator.
        /// </summary>
        private readonly StructBuffer<T1> structBuffer;

        /// <summary>
        ///     Current index into the buffer.
        /// </summary>
        private int currentIndex;

        public StructBufferEnumerator(StructBuffer<T1> structBuffer)
        {
            this.structBuffer = structBuffer;
            currentIndex = -1;
        }

        public T1 Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            Monitor.Exit(structBuffer.list);
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