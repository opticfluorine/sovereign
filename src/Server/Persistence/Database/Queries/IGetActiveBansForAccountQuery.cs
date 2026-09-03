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
using Sovereign.Persistence.Bans;

namespace Sovereign.Persistence.Database.Queries;

/// <summary>
///     Query for retrieving the active bans of an account.
/// </summary>
public interface IGetActiveBansForAccountQuery
{
    /// <summary>
    ///     Gets the active bans for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <returns>Active bans for the account.</returns>
    List<BanInfo> GetActiveBansForAccount(Guid accountId);
}
