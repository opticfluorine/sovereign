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

namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Defines delegate types for component add/remove/modify events.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public static class ComponentEventDelegates<T>
    {

        /// <summary>
        /// Delegate type used to communicate component add and update events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="componentValue">New component value.</param>
        public delegate void ComponentEventHandler(ulong entityId, T componentValue);

        /// <summary>
        /// Delegate type used to communicate component remove events.
        /// </summary>
        /// <param name="entityId">Entity ID.</param>
        public delegate void ComponentRemovedEventHandler(ulong entityId);

    }

}
