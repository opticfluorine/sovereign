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

using System.Collections.Generic;
using System.Data;
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for retrieving the flags of every world segment within an inclusive box.
/// </summary>
public interface IGetSegmentFlagsInRangeQuery
{
    /// <summary>
    ///     Gets the stored flags for all world segments within the given inclusive box.
    /// </summary>
    /// <param name="min">Minimum (inclusive) corner of the box.</param>
    /// <param name="max">Maximum (inclusive) corner of the box.</param>
    /// <param name="transaction">
    ///     Optional transaction to participate in. Required when the connection has a pending
    ///     transaction, such as during synchronization.
    /// </param>
    /// <returns>Stored flags keyed by segment index; segments without a stored row are omitted.</returns>
    IDictionary<GridPosition, WorldSegmentFlag> GetFlagsInRange(GridPosition min, GridPosition max,
        IDbTransaction? transaction = null);
}
