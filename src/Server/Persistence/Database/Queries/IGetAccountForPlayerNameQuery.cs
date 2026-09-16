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

using System;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for resolving a player name to the associated account ID.
/// </summary>
public interface IGetAccountForPlayerNameQuery
{
    /// <summary>
    ///     Tries to resolve a player name to the associated account ID.
    /// </summary>
    /// <param name="playerName">Player name (matched case-insensitively).</param>
    /// <param name="accountId">Account ID. Only valid if the method returns true.</param>
    /// <returns>true if the account was found, false otherwise.</returns>
    bool TryGetAccountForPlayer(string playerName, out Guid accountId);
}
