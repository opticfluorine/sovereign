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

namespace Sovereign.ServerNetwork.Network.Rest;

/// <summary>
///     Sovereign Engine REST API authentication constants.
/// </summary>
public static class RestAuthentication
{
    /// <summary>
    ///     Sovereign Engine claim types.
    /// </summary>
    public static class ClaimTypes
    {
        /// <summary>
        ///     AccountId claim type.
        /// </summary>
        public const string AccountId = "urn:sovereign:accountid";

        /// <summary>
        ///     PlayerId claim type.
        /// </summary>
        public const string PlayerId = "urn:sovereign:playerid";
    }
}

/// <summary>
///     Sovereign Engine REST API authorization constants.
/// </summary>
public static class RestAuthorization
{
    /// <summary>
    ///     Authorization policy names.
    /// </summary>
    public static class Policies
    {
        /// <summary>
        ///     Admin Only policy. Requires the Admin role.
        /// </summary>
        public const string AdminOnly = nameof(AdminOnly);

        /// <summary>
        ///     Require Player policy. Requires that the user has entered the world as a player character.
        /// </summary>
        public const string RequirePlayer = nameof(RequirePlayer);

        /// <summary>
        ///     Require Out of Game policy. Requires that the user has not yet entered the world.
        /// </summary>
        public const string RequireOutOfGame = nameof(RequireOutOfGame);

        /// <summary>
        ///     Default authentication policy. Requires account authentication.
        /// </summary>
        public const string Default = nameof(Default);
    }

    /// <summary>
    ///     Authorization role names.
    /// </summary>
    public static class Roles
    {
        /// <summary>
        ///     Admin role. Indicates that the user's player has the Admin role.
        /// </summary>
        public const string Admin = nameof(Admin);

        /// <summary>
        ///     Player role. Indicates that the user has entered the game world as a player character.
        /// </summary>
        public const string Player = nameof(Player);

        /// <summary>
        ///     Out Of Game role. Indicates that the user has not entered the game world (i.e. is in the menus).
        /// </summary>
        public const string OutOfGame = nameof(OutOfGame);
    }
}