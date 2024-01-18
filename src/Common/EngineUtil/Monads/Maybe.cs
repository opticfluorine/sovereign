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
using System.Runtime.Serialization;

namespace Sovereign.EngineUtil.Monads;

/// <summary>
///     Implementation of the "Maybe" monad.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
public struct Maybe<T> where T : notnull
{
    /// <summary>
    ///     Underlying value.
    /// </summary>
    private T? value;

    /// <summary>
    ///     Creates a Maybe with no underlying value.
    /// </summary>
    public Maybe()
    {
        value = default;
        HasValue = false;
    }

    /// <summary>
    ///     Creates a Maybe with an underlying value.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public Maybe(T value)
    {
        this.value = value;
        HasValue = true;
    }

    /// <summary>
    ///     Whether a value is present.
    /// </summary>
    public bool HasValue { get; private set; }

    /// <summary>
    ///     Provides access to the underlying value.
    /// </summary>
    /// <exception cref="NoValueException">
    ///     Thrown if the property getter is invoked when there is no value present.
    /// </exception>
    public T Value
    {
        get
        {
            if (!HasValue || value == null) throw new NoValueException();
            return value;
        }

        set
        {
            this.value = value;
            HasValue = true;
        }
    }

    /// <summary>
    ///     Gets the underlying value, or returns a default value if
    ///     no underlying value is present.
    /// </summary>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>Underlying value, or defaultValue if no value is present.</returns>
    public T OrElseDefault(T defaultValue)
    {
        return HasValue ? Value : defaultValue;
    }

    /// <summary>
    ///     Gets the underlying value, or returns the value provided by another
    ///     function if no underlying value is present.
    /// </summary>
    /// <param name="supplier">Function to supply a value if none present.</param>
    /// <returns>Value, or the return value of supplier if no value is present.</returns>
    public T OrElse(Func<T> supplier)
    {
        return HasValue ? Value : supplier();
    }

    /// <summary>
    ///     Exception type thrown when no value is present.
    /// </summary>
    [Serializable]
    public class NoValueException : Exception
    {
        public NoValueException()
        {
        }

        public NoValueException(string message) : base(message)
        {
        }

        public NoValueException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NoValueException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}