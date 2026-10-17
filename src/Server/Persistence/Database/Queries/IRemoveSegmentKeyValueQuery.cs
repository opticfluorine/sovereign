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

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for removing a key-value pair of a single world segment.
/// </summary>
public interface IRemoveSegmentKeyValueQuery
{
    /// <summary>
    ///     Removes the key-value pair for the given world segment if it exists.
    /// </summary>
    /// <param name="segmentIndex">World segment index.</param>
    /// <param name="key">Key.</param>
    /// <param name="transaction">Transaction.</param>
    void RemoveKeyValue(GridPosition segmentIndex, string key, IDbTransaction transaction);
}
