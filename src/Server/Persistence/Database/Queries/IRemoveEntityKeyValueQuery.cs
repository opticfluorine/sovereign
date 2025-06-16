// Sovereign Engine
// Copyright (c) 2025 opticfluorine
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
///     Query for removing key-value pairs for entities.
/// </summary>
public interface IRemoveEntityKeyValueQuery
{
    /// <summary>
    ///     Removes the key-value pair for the given entity if it exists.
    /// </summary>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="key">Key.</param>
    /// <param name="transaction">Transaction.</param>
    void RemoveEntityKeyValue(ulong entityId, string key, IDbTransaction transaction);
}