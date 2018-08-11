﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Engine8.EngineUtil.Collections
{

    /// <summary>
    /// Maintains a reusable buffer of structs.
    /// </summary>
    ///
    /// This class is thread-safe. Note that generic enumerators for a StructBuffer lock
    /// the buffer for their entire existence; the lock is only released when the
    /// enumerator's Dispose() method is called. Note that this means that enumerators
    /// cannot be shared between threads. If cross-threaded enumeration is needed, first
    /// rethink your design, and if you still need it, use the old-style enumerator.
    /// 
    /// <typeparam name="T">Struct type. Must be mutable.</typeparam>
    /// <inheritdoc />
    public class StructBuffer<T> : IEnumerable<T> where T : struct
    {

        /// <summary>
        /// Backing list.
        /// </summary>
        private List<T> list;

        /// <summary>
        /// Buffer capacity.
        /// </summary>
        public int Capacity => list.Capacity;

        /// <summary>
        /// Number of active objects in the buffer.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Creates a buffer with the given number of objects.
        /// </summary>
        /// <param name="capacity">Capacity.</param>
        public StructBuffer(int capacity)
        {
            list = new List<T>(capacity);
        }

        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item">Adds the item to the buffer.</param>
        public void Add(ref T item)
        {
            Monitor.Enter(list);
            try
            {
                list[Count++] = item;
            }
            finally
            {
                Monitor.Exit(list);
            }
        }

        /// <summary>
        /// Resets the buffer.
        /// </summary>
        public void Clear()
        {
            Monitor.Enter(list);
            Count = 0;
            Monitor.Exit(list);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Monitor.Enter(list);
            return new StructBufferEnumerator<T>(this);
        }

        /// <summary>
        /// This operation is not supported - use the generic version instead.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generic enumerator for StructBuffer.
        /// </summary>
        /// <typeparam name="T1">Element type of the underlying StructBuffer.</typeparam>
        /// <inheritdoc />
        public class StructBufferEnumerator<T1> : IEnumerator<T1> where T1 : struct
        {

            /// <summary>
            /// Struct buffer associated with this enumerator.
            /// </summary>
            private StructBuffer<T1> structBuffer;

            /// <summary>
            /// Current index into the buffer.
            /// </summary>
            private int currentIndex;

            public T1 Current { get; private set; }

            object IEnumerator.Current => (object)Current;

            public StructBufferEnumerator(StructBuffer<T1> structBuffer)
            {
                this.structBuffer = structBuffer;
            }

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

}
