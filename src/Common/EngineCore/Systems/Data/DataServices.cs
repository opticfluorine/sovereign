// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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

using System;
using System.Collections.Generic;

namespace Sovereign.EngineCore.Systems.Data;

/// <summary>
///     Interface for read-only services exposed by the Data system.
/// </summary>
/// <remarks>
///     Implementations of this interface must be thread-safe.
/// </remarks>
public interface IDataServices
{
    /// <summary>
    ///     Checks if the given global key exists.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <returns>true if exists, false otherwise.</returns>
    bool HasGlobal(string key);

    /// <summary>
    ///     Gets the string value associated with the given global key.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <returns>Associated value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    string GetGlobal(string key);

    /// <summary>
    ///     Gets the value associated with the given global key, converting the value from string
    ///     to the specified type.
    /// </summary>
    /// <param name="key">Global key.</param>
    /// <typeparam name="T">Desired value type.</typeparam>
    /// <returns>Associated value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the given key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to the given type.</exception>
    T GetGlobal<T>(string key);
}

/// <summary>
///     Implementation of IDataServices.
/// </summary>
public class DataServices : IDataServices
{
    public bool HasGlobal(string key)
    {
        return false;
    }

    public string GetGlobal(string key)
    {
        throw new KeyNotFoundException();
    }

    public T GetGlobal<T>(string key)
    {
        throw new KeyNotFoundException();
    }
}