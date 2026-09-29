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
///     Query for deleting world segment block data within a world-aligned segment box.
/// </summary>
public interface IDeleteWorldSegmentBlockDataInRangeQuery
{
    /// <summary>
    ///     Deletes all world segment block data rows whose segment coordinates fall within
    ///     the given segment-coordinate box.
    /// </summary>
    /// <param name="minX">Inclusive minimum segment X.</param>
    /// <param name="maxX">Exclusive maximum segment X.</param>
    /// <param name="minY">Inclusive minimum segment Y.</param>
    /// <param name="maxY">Exclusive maximum segment Y.</param>
    /// <param name="minZ">Inclusive minimum segment Z.</param>
    /// <param name="maxZ">Exclusive maximum segment Z.</param>
    /// <param name="transaction">Transaction.</param>
    void DeleteInRange(int minX, int maxX, int minY, int maxY, int minZ, int maxZ,
        IDbTransaction transaction);
}
