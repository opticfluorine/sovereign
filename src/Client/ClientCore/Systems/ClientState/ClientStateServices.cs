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
    private readonly MainMenuStateMachine mainMenuStateMachine;
    private readonly PlayerStateManager playerStateManager;
    private readonly ClientStateMachine stateMachine;

    public ClientStateServices(ClientStateMachine stateMachine, ClientStateFlagManager flagManager,
        PlayerStateManager playerStateManager, MainMenuStateMachine mainMenuStateMachine)
    {
        this.stateMachine = stateMachine;
        this.flagManager = flagManager;
        this.playerStateManager = playerStateManager;
        this.mainMenuStateMachine = mainMenuStateMachine;
    }

    /// <summary>
    ///     Current state of the top-level client state machine.
    /// </summary>
    public MainClientState State => stateMachine.State;

    /// <summary>
    ///     Current state of the main menu.
    /// </summary>
    public MainMenuState MainMenuState => mainMenuStateMachine.State;

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
    ///     Checks the value of a flag and clears it if currently set.
    /// </summary>
    /// <param name="flag">Flag.</param>
    /// <returns>Flag state.</returns>
    public bool CheckAndClearFlagValue(ClientStateFlag flag)
    {
        var set = flagManager.GetStateFlagValue(flag);
        if (set) flagManager.SetStateFlagValue(flag, false);
        return set;
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

    /// <summary>
    ///     Gets the value of the main menu reset flag, then clears it.
    /// </summary>
    /// <returns>Main menu reset flag.</returns>
    public bool CheckAndClearMainMenuResetFlag()
    {
        var value = mainMenuStateMachine.NeedReset;
        mainMenuStateMachine.ClearReset();
        return value;
    }
}