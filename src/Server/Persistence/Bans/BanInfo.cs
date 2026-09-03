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

namespace Sovereign.Persistence.Bans;

/// <summary>
///     Data class for an account ban.
/// </summary>
public sealed class BanInfo
{
    /// <summary>
    ///     Banned account ID.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Username of the banned account.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    ///     Player name for which the ban was recorded.
    /// </summary>
    public string PlayerName { get; set; } = "";

    /// <summary>
    ///     Player name of the admin who created the ban.
    /// </summary>
    public string AdminName { get; set; } = "";

    /// <summary>
    ///     UTC timestamp at which the ban was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    ///     Ban duration in days, or null for a permanent ban.
    /// </summary>
    public int? DurationDays { get; set; }
}
