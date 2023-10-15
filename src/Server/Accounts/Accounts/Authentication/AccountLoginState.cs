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

namespace Sovereign.Accounts.Accounts.Authentication;

/// <summary>
///     Enumeration of account login states.
/// </summary>
public enum AccountLoginState
{
    /// <summary>
    ///     Account is not logged in.
    /// </summary>
    NotLoggedIn,

    /// <summary>
    ///     Account is authenticated but a player character has not yet been selected.
    /// </summary>
    SelectingPlayer,

    /// <summary>
    ///     Account is authenticated and is in game with a player character.
    /// </summary>
    InGame
}