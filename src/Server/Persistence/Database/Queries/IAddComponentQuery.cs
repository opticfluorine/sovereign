/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
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

using System.Data;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Generic query interface for adding a component.
/// </summary>
/// <typeparam name="T">Component value type.</typeparam>
public interface IAddComponentQuery<T>
{
    /// <summary>
    ///     Adds a component to the database.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="value">Component value.</param>
    /// <param name="transaction">Database transaction.</param>
    void Add(ulong entityId, T value, IDbTransaction transaction);
}