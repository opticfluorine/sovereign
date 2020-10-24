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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.EngineUtil.Collections
{

    /// <summary>
    /// Interface implemented by heap data structures.
    /// </summary>
    /// <typeparam name="T">Data type stored in this heap.</typeparam>
    public interface IHeap<T> : ICollection<T>
    {

        /// <summary>
        /// Pushes an object onto the heap.
        /// </summary>
        /// <param name="obj">Object to push onto the heap.</param>
        void Push(T obj);

        /// <summary>
        /// Pops the minimum object on the heap.
        /// </summary>
        /// <returns>Minimum object on the heap.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the heap is empty.
        /// </exception>
        T Pop();

        /// <summary>
        /// Peeks at the minimum object on the heap, but does not remove it.
        /// </summary>
        /// <returns>Minimum object on the heap.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the heap is empty.
        /// </exception>
        T Peek();

    }

}
