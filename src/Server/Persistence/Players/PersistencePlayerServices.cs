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
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Players;

/// <summary>
///     Public API exported by Persistence to provide player-related database services.
/// </summary>
public class PersistencePlayerServices
{
    private readonly IPersistenceProvider provider;

    public PersistencePlayerServices(PersistenceProviderManager manager)
    {
        provider = manager.PersistenceProvider;
    }

    /// <summary>
    ///     Determines whether the given player name is taken.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <returns>true if taken, false otherwise.</returns>
    /// <remarks>
    ///     Note that this only checks whether the name exists in the database for a player character.
    ///     It does not check whether a player character with the same name exists in memory and has not
    ///     yet been synchronized to the database.
    /// </remarks>
    public bool IsPlayerNameTaken(string name)
    {
        return provider.PlayerExistsQuery.PlayerExists(name);
    }

    /// <summary>
    ///     Determines whether the given entity is a player character associated to the given account.
    /// </summary>
    /// <param name="playerEntityId">Entity ID to check.</param>
    /// <param name="accountId">Account to check.</param>
    /// <returns>true if entity is a player character belonging to the given account, false otherwise.</returns>
    /// <remarks>
    ///     Note that this only checks whether the relationship exists in the database for a player character.
    ///     It does not check accounts and/or players that are not yet synchronized to the database.
    /// </remarks>
    public bool ValidatePlayerAccountPair(ulong playerEntityId, Guid accountId)
    {
        return provider.GetAccountForPlayerQuery.TryGetAccountForPlayer(playerEntityId, out var foundAccountId)
               && foundAccountId.Equals(accountId);
    }
}