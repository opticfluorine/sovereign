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

using Sovereign.EngineCore.Components.Indexers;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for retrieving world segment block data.
/// </summary>
public interface IGetWorldSegmentBlockDataQuery
{
    /// <summary>
    ///     Gets the block data associated with the given world segment if any is present.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="buffer">
    ///     Buffer to hold the serialized uncompressed block data.
    ///     It is assumed that the buffer is large enough to hold the largest possible
    ///     uncompressed serialized block data structure (slightly larger than 512 KB, so
    ///     1 MB is recommended).
    /// </param>
    /// <returns>true if block data was present, false otherwise.</returns>
    bool TryGetWorldSegmentBlockData(GridPosition segmentIndex, byte[] buffer);
}