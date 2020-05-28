/*
 * Sovereign Engine
 * Copyright (c) 2020 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
