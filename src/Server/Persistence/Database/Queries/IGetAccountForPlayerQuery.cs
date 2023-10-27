// Sovereign Engine
// Copyright (c) 2023 opticfluorine
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
///     Query interface for retrieving the account ID associated with a player character.
/// </summary>
public interface IGetAccountForPlayerQuery
{
    /// <summary>
    ///     Gets the account ID associated with a player ID.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <param name="accountId">Account ID. Only valid if method returns true.</param>
    /// <returns>true if the player character exists and the account is associated; false otherwise.</returns>
    bool TryGetAccountForPlayer(ulong playerEntityId, out Guid accountId);
}