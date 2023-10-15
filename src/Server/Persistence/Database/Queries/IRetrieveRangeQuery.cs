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

using System.Numerics;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Interface for retrieving all entities in a position range.
/// </summary>
public interface IRetrieveRangeQuery
{
    /// <summary>
    ///     Queries the database for all entities in the given range.
    /// </summary>
    /// <param name="minPos">Minimum position.</param>
    /// <param name="maxPos">Maximum position.</param>
    /// <returns></returns>
    QueryReader RetrieveEntitiesInRange(Vector3 minPos, Vector3 maxPos);
}