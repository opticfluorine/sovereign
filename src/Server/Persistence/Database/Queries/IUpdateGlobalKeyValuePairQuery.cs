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
///     Query for updating (creating or modifying) a global key-value pair.
/// </summary>
public interface IUpdateGlobalKeyValuePairQuery
{
    /// <summary>
    ///     Updates the given global key-value pair.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value in string representation.</param>
    /// <param name="transaction">Transaction.</param>
    void UpdateGlobalKeyValuePair(string key, string value, IDbTransaction transaction);
}