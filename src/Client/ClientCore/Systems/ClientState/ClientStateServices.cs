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

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Exports public services from the ClientState system to the rest of the client.
/// </summary>
public class ClientStateServices
{
    private readonly ClientStateFlagManager flagManager;
    private readonly PlayerStateManager playerStateManager;
    private readonly ClientStateMachine stateMachine;

    public ClientStateServices(ClientStateMachine stateMachine, ClientStateFlagManager flagManager,
        PlayerStateManager playerStateManager)
    {
        this.stateMachine = stateMachine;
        this.flagManager = flagManager;
        this.playerStateManager = playerStateManager;
    }

    /// <summary>
    ///     Current state of the top-level client state machine.
    /// </summary>
    public MainClientState State => stateMachine.State;

    /// <summary>
    ///     Gets the current value of a state flag.
    /// </summary>
    /// <param name="flag">Flag.</param>
    /// <returns>Current value.</returns>
    public bool GetStateFlagValue(ClientStateFlag flag)
    {
        return flagManager.GetStateFlagValue(flag);
    }

    /// <summary>
    ///     Gets the entity ID of the currently selected player, if any.
    /// </summary>
    /// <param name="playerEntityId">Player entity ID, only valid if method returns true.</param>
    /// <returns>true if a player has been selected, false otherwise.</returns>
    public bool TryGetSelectedPlayer(out ulong playerEntityId)
    {
        return playerStateManager.TryGetPlayerEntityId(out playerEntityId);
    }
}