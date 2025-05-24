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

using Sovereign.EngineUtil.Monads;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Manages client-side player state.
/// </summary>
public class PlayerStateManager
{
    private const ulong NoPlayer = 0;
    
    /// <summary>
    ///     Current player entity ID. Empty if a player is not logged in.
    /// </summary>
    private ulong playerEntityId = NoPlayer;

    /// <summary>
    ///     Updates state for a player selection/login.
    /// </summary>
    /// <param name="playerEntityId">Selected player entity ID.</param>
    public void PlayerSelected(ulong playerEntityId)
    {
        this.playerEntityId = playerEntityId;
    }

    /// <summary>
    ///     Updates state for a player logout.
    /// </summary>
    public void PlayerLogout()
    {
        playerEntityId = NoPlayer;
    }

    /// <summary>
    ///     Gets the selected player entity ID if a player has been selected.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID, only valid if method returns true.</param>
    /// <returns>true if a player has been selected, false otherwise.</returns>
    public bool TryGetPlayerEntityId(out ulong playerEntityId)
    {
        playerEntityId = this.playerEntityId;
        return this.playerEntityId != NoPlayer;
    }
}