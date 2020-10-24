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
using System.Collections.Concurrent;

namespace Sovereign.EngineUtil.Collections
{

    /// <summary>
    /// Simple object pool.
    /// </summary>
    /// <typeparam name="T">Type to object to pool.</typeparam>
    public sealed class ObjectPool<T>
    {

        /// <summary>
        /// Default size of the object pool.
        /// </summary>
        public const int DefaultSize = 32;

        private readonly ConcurrentBag<T> bag;

        private readonly Func<T> producer;

        public ObjectPool(Func<T> producer, int size = DefaultSize)
        {
            this.producer = producer;
            bag = new ConcurrentBag<T>();
            for (int i = 0; i < size; ++i)
            {
                ReturnObject(producer());
            }
        }

        public ObjectPool(int size = DefaultSize)
            : this(() => Activator.CreateInstance<T>(), size)
        {

        }

        /// <summary>
        /// Takes an object from the pool, creating a new object if none are available.
        /// </summary>
        /// <returns>Pooled object.</returns>
        public T TakeObject()
        {
            if (bag.TryTake(out T obj))
                return obj;
            else
                return producer();
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="t">Pooled object.</param>
        public void ReturnObject(T t)
        {
            bag.Add(t);
        }

    }

}
