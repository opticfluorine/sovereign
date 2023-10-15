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
using System.Collections.Concurrent;

namespace Sovereign.EngineUtil.Collections;

/// <summary>
///     Simple object pool.
/// </summary>
/// <typeparam name="T">Type to object to pool.</typeparam>
public sealed class ObjectPool<T>
{
    /// <summary>
    ///     Default size of the object pool.
    /// </summary>
    public const int DefaultSize = 32;

    private readonly ConcurrentBag<T> bag;

    private readonly Func<T> producer;

    public ObjectPool(Func<T> producer, int size = DefaultSize)
    {
        this.producer = producer;
        bag = new ConcurrentBag<T>();
        for (var i = 0; i < size; ++i) ReturnObject(producer());
    }

    public ObjectPool(int size = DefaultSize)
        : this(() => Activator.CreateInstance<T>(), size)
    {
    }

    /// <summary>
    ///     Takes an object from the pool, creating a new object if none are available.
    /// </summary>
    /// <returns>Pooled object.</returns>
    public T TakeObject()
    {
        if (bag.TryTake(out var obj))
            return obj;
        return producer();
    }

    /// <summary>
    ///     Returns an object to the pool.
    /// </summary>
    /// <param name="t">Pooled object.</param>
    public void ReturnObject(T t)
    {
        bag.Add(t);
    }
}