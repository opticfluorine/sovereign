/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

namespace Sovereign.EngineUtil.Monads
{

    /// <summary>
    /// Option type.
    /// </summary>
    public sealed class Option<T1, T2>
    {

        /// <summary>
        /// Backing maybe for first type.
        /// </summary>
        private Maybe<T1> t1maybe;

        /// <summary>
        /// Backing maybe for second type.
        /// </summary>
        private Maybe<T2> t2maybe;

        /// <summary>
        /// Whether this option holds an object of the first type.
        /// </summary>
        public bool HasFirst { get { return t1maybe.HasValue; } }

        /// <summary>
        /// Whether this option holds an object of the second type.
        /// </summary>
        public bool HasSecond { get { return t2maybe.HasValue; } }

        /// <summary>
        /// Object of the first type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this option does not hold an object of the first type.</exception>
        public T1 First { get { if (HasFirst) return t1maybe.Value; else throw new InvalidOperationException("Option does not hold first type."); } }

        /// <summary>
        /// Object of the second type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this option does not hold an object of the second type.</exception>
        public T2 Second { get { if (HasSecond) return t2maybe.Value; else throw new InvalidOperationException("Option does not hold second type."); } }

        /// <summary>
        /// Creates an option with the given value.
        /// </summary>
        /// <param name="value">Value of the first type.</param>
        public Option(T1 value)
        {
            t1maybe = new Maybe<T1>(value);
            t2maybe = new Maybe<T2>();
        }

        /// <summary>
        /// Creates an option with the given value.
        /// </summary>
        /// <param name="value">Value of the second type.</param>
        public Option(T2 value)
        {
            t1maybe = new Maybe<T1>();
            t2maybe = new Maybe<T2>(value);
        }

    }

}
