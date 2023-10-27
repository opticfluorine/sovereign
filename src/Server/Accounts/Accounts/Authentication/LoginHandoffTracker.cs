/*
 * Sovereign Engine
 * Copyright (c) 2019 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Tracks connection handoffs after successful login.
/// </summary>
public sealed class LoginHandoffTracker
{
    /// <summary>
    ///     Account IDs with pending handoffs.
    /// </summary>
    private readonly HashSet<Guid> pendingHandoffs = new();

    /// <summary>
    ///     Adds an account to the pending handoff list.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <remarks>
    ///     If the account is already in the pending handoff list, this
    ///     method has no effect.
    /// </remarks>
    public void AddPendingHandoff(Guid accountId)
    {
        pendingHandoffs.Add(accountId);
    }

    /// <summary>
    ///     Removes an account from the pending handoff list if it is
    ///     present.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>true if the account was in the list; false otherwise.</returns>
    public bool TakePendingHandoff(Guid accountId)
    {
        return pendingHandoffs.Remove(accountId);
    }
}