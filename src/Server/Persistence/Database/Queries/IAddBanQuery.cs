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
///     Query for adding a ban to an account.
/// </summary>
public interface IAddBanQuery
{
    /// <summary>
    ///     Adds a ban for the given account.
    /// </summary>
    /// <param name="accountId">Account ID.</param>
    /// <param name="playerName">Player name the ban was recorded for.</param>
    /// <param name="adminName">Player name of the admin who created the ban.</param>
    /// <param name="createdUtc">UTC timestamp at which the ban was created.</param>
    /// <param name="durationDays">Ban duration in days, or null for a permanent ban.</param>
    void AddBan(Guid accountId, string playerName, string adminName,
        DateTime createdUtc, int? durationDays);
}
