// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Interface for adding or modifying world segment block data in the database.
/// </summary>
public interface ISetWorldSegmentBlockDataQuery
{
    /// <summary>
    ///     Adds or updates the world segment block data for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="data">Uncompressed serialized block data created from WorldSegmentBlockData.</param>
    /// <param name="transaction">Transaction.</param>
    void SetWorldSegmentBlockData(GridPosition segmentIndex, byte[] data, IDbTransaction transaction);
}