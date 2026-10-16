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
using Sovereign.EngineCore.Components.Types;
using Sovereign.EngineCore.Systems.Data;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for creating and updating a key-value pair of a single world segment.
/// </summary>
public interface ISetSegmentKeyValueQuery
{
    /// <summary>
    ///     Creates or updates the key-value pair for the given world segment.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Typed value.</param>
    /// <param name="transaction">Transaction.</param>
    void SetKeyValue(GridPosition segmentIndex, string key, DataValue value, IDbTransaction transaction);
}
