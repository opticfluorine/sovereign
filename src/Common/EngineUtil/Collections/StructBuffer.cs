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
    ///     Reference indexer to the underlying buffer entries.
    /// </summary>
    /// <param name="index">Index.</param>
    public ref T this[int index] => ref list[index];

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
        for (var i = 0; i < Count; ++i)
        {
            yield return list[i];
        }
    }

    /// <summary>
    ///     This operation is not supported - use the generic version instead.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
}