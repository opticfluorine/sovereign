// Sovereign Engine
// Copyright (c) 2026 opticfluorine
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

using System.Collections;
using System.Collections.Generic;

namespace Sovereign.EngineUtil.Collections;

/// <summary>
///     Set that preserves the insertion order of its elements during enumeration.
/// </summary>
/// <typeparam name="T">Type of element stored in the set.</typeparam>
public sealed class LinkedHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly List<T> order = new();
    private readonly HashSet<T> seen;

    /// <summary>
    ///     Creates an empty set using the default equality comparer.
    /// </summary>
    public LinkedHashSet() : this(null)
    {
    }

    /// <summary>
    ///     Creates an empty set using the given equality comparer.
    /// </summary>
    /// <param name="comparer">Equality comparer, or null for the default comparer.</param>
    public LinkedHashSet(IEqualityComparer<T>? comparer)
    {
        seen = new HashSet<T>(comparer);
    }

    /// <summary>
    ///     Number of elements in the set.
    /// </summary>
    public int Count => order.Count;

    /// <summary>
    ///     Adds an element if it is not already present.
    /// </summary>
    /// <param name="item">Element to add.</param>
    /// <returns>true if the element was added, false if it was already present.</returns>
    public bool Add(T item)
    {
        if (!seen.Add(item)) return false;

        order.Add(item);
        return true;
    }

    /// <summary>
    ///     Checks whether the set contains the given element.
    /// </summary>
    /// <param name="item">Element to find.</param>
    /// <returns>true if the element is present, false otherwise.</returns>
    public bool Contains(T item)
    {
        return seen.Contains(item);
    }

    /// <summary>
    ///     Enumerates the elements in insertion order.
    /// </summary>
    /// <returns>Enumerator.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return order.GetEnumerator();
    }

    /// <summary>
    ///     Enumerates the elements in insertion order.
    /// </summary>
    /// <returns>Enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
