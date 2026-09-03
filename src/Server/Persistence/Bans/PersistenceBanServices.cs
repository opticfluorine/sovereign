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
using System.Collections.Generic;
using Sovereign.Persistence.Database;

namespace Sovereign.Persistence.Bans;

/// <summary>
///     Public API exported by Persistence to provide ban-related database services.
/// </summary>
public class PersistenceBanServices
{
    private readonly IPersistenceProvider provider;

    public PersistenceBanServices(PersistenceProviderManager manager)
    {
        provider = manager.PersistenceProvider;
    }

    /// <summary>
    ///     Tries to resolve a player name to the associated account ID.
    /// </summary>
    /// <param name="playerName">Player name (matched case-insensitively).</param>
    /// <param name="accountId">Account ID. Only valid if the method returns true.</param>
    /// <returns>true if the account was found, false otherwise.</returns>
    public bool TryGetAccountForPlayerName(string playerName, out Guid accountId)
    {
        provider.TransactionLock.Acquire();
        try
        {
            return provider.GetAccountForPlayerNameQuery.TryGetAccountForPlayer(playerName,
                out accountId);
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Adds a ban for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerName">Player name the ban is recorded for.</param>
    /// <param name="adminName">Player name of the admin who created the ban.</param>
    /// <param name="durationDays">Ban duration in days, or null for a permanent ban.</param>
    public void AddBan(Guid accountId, string playerName, string adminName, int? durationDays)
    {
        provider.TransactionLock.Acquire();
        try
        {
            provider.AddBanQuery.AddBan(accountId, playerName, adminName, DateTime.UtcNow,
                durationDays);
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Soft deletes all active bans for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Number of bans that were removed.</returns>
    public int RemoveBansForAccount(Guid accountId)
    {
        provider.TransactionLock.Acquire();
        try
        {
            return provider.RemoveBansForAccountQuery.RemoveBansForAccount(accountId);
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Gets the active bans for the given account, soft deleting any expired bans first.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Active bans for the account.</returns>
    public List<BanInfo> GetActiveBansForAccount(Guid accountId)
    {
        provider.TransactionLock.Acquire();
        try
        {
            provider.SoftDeleteExpiredBansQuery.SoftDeleteExpiredBans();
            return provider.GetActiveBansForAccountQuery.GetActiveBansForAccount(accountId);
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }

    /// <summary>
    ///     Gets the active bans for all accounts, soft deleting any expired bans first.
    /// </summary>
    /// <returns>Active bans for all accounts.</returns>
    public List<BanInfo> GetActiveBans()
    {
        provider.TransactionLock.Acquire();
        try
        {
            provider.SoftDeleteExpiredBansQuery.SoftDeleteExpiredBans();
            return provider.ListActiveBansQuery.ListActiveBans();
        }
        finally
        {
            provider.TransactionLock.Release();
        }
    }
}
