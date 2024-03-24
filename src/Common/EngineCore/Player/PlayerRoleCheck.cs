// Sovereign Engine
// Copyright (c) 2024 opticfluorine
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

using Sovereign.EngineCore.Components;

namespace Sovereign.EngineCore.Player;

/// <summary>
///     Provides utility methods for checking if a player has a given role.
/// </summary>
public class PlayerRoleCheck
{
    private readonly AdminTagCollection admins;

    public PlayerRoleCheck(AdminTagCollection admins)
    {
        this.admins = admins;
    }

    /// <summary>
    ///     Checks whether the given player is an admin.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID.</param>
    /// <returns>true if admin, false otherwise.</returns>
    public bool IsPlayerAdmin(ulong playerEntityId)
    {
        return admins.HasComponentForEntity(playerEntityId) && admins[playerEntityId];
    }
}