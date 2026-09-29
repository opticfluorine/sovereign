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

using System.Data;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for deleting worldgen-placed decoration entities within a world-aligned box.
/// </summary>
public interface IDeleteWorldGenDecorationsInRangeQuery
{
    /// <summary>
    ///     Deletes all templated NPC entities positioned within the given world-aligned
    ///     box. Players and entities without a position or template are never deleted.
    /// </summary>
    /// <param name="minX">Inclusive minimum world X.</param>
    /// <param name="maxX">Exclusive maximum world X.</param>
    /// <param name="minY">Inclusive minimum world Y.</param>
    /// <param name="maxY">Exclusive maximum world Y.</param>
    /// <param name="minZ">Inclusive minimum world Z.</param>
    /// <param name="maxZ">Exclusive maximum world Z.</param>
    /// <param name="transaction">Transaction.</param>
    /// <returns>Number of entities deleted.</returns>
    int DeleteInRange(float minX, float maxX, float minY, float maxY, float minZ, float maxZ,
        IDbTransaction transaction);
}
