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

using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Updater;

namespace Sovereign.ClientCore.Systems.ClientState;

/// <summary>
///     Detects the state change associated with the end of autoupdater operations.
/// </summary>
public class AutoUpdaterEndDetector
{
    private readonly AutoUpdater autoUpdater;
    private readonly ClientConfigurationManager configManager;
    private readonly ClientStateMachine stateMachine;


    public AutoUpdaterEndDetector(AutoUpdater autoUpdater, ClientConfigurationManager configManager,
        ClientStateMachine stateMachine)
    {
        this.autoUpdater = autoUpdater;
        this.configManager = configManager;
        this.stateMachine = stateMachine;
    }

    /// <summary>
    ///     Called for each system tick during the Update client state.
    /// </summary>
    public void OnTick()
    {
        // Sanity check.
        if (stateMachine.State != MainClientState.Update) return;

        if (!configManager.ClientConfiguration.AutoUpdater.UpdateOnStartup ||
            autoUpdater.State == AutoUpdaterState.Complete)
            stateMachine.TryTransition(MainClientState.MainMenu);
    }
}