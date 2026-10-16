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
using Sovereign.EngineCore.Components.Types;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     A single raw world segment key-value row as stored in the database.
/// </summary>
/// <param name="Key">Key.</param>
/// <param name="Value">Canonical text encoding of the value.</param>
/// <param name="Type">Type tag using the DataValueType numbering.</param>
public readonly record struct SegmentKeyValueRow(string Key, string Value, int Type);

/// <summary>
///     Query for retrieving all key-value pairs of a single world segment.
/// </summary>
public interface IGetSegmentKeyValuesQuery
{
    /// <summary>
    ///     Gets every stored key-value row for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <returns>Raw key-value rows for the segment, empty if none are stored.</returns>
    IReadOnlyList<SegmentKeyValueRow> GetKeyValues(GridPosition segmentIndex);
}
