/*
 * Sovereign Engine
 * Copyright (c) 2023 opticfluorine
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

namespace Sovereign.EngineUtil.Monads;

/// <summary>
///     Option type.
/// </summary>
public sealed class Option<T1, T2>
{
    /// <summary>
    ///     Backing maybe for first type.
    /// </summary>
    private readonly Maybe<T1> t1maybe;

    /// <summary>
    ///     Backing maybe for second type.
    /// </summary>
    private readonly Maybe<T2> t2maybe;

    /// <summary>
    ///     Creates an option with the given value.
    /// </summary>
    /// <param name="value">Value of the first type.</param>
    public Option(T1 value)
    {
        t1maybe = new Maybe<T1>(value);
        t2maybe = new Maybe<T2>();
    }

    /// <summary>
    ///     Creates an option with the given value.
    /// </summary>
    /// <param name="value">Value of the second type.</param>
    public Option(T2 value)
    {
        t1maybe = new Maybe<T1>();
        t2maybe = new Maybe<T2>(value);
    }

    /// <summary>
    ///     Whether this option holds an object of the first type.
    /// </summary>
    public bool HasFirst => t1maybe.HasValue;

    /// <summary>
    ///     Whether this option holds an object of the second type.
    /// </summary>
    public bool HasSecond => t2maybe.HasValue;

    /// <summary>
    ///     Object of the first type.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this option does not hold an object of the first type.</exception>
    public T1 First
    {
        get
        {
            if (HasFirst) return t1maybe.Value;
            throw new InvalidOperationException("Option does not hold first type.");
        }
    }

    /// <summary>
    ///     Object of the second type.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this option does not hold an object of the second type.</exception>
    public T2 Second
    {
        get
        {
            if (HasSecond) return t2maybe.Value;
            throw new InvalidOperationException("Option does not hold second type.");
        }
    }
}