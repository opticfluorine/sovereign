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

using ImGuiNET;
using Sovereign.ClientCore.Configuration;
using Sovereign.ClientCore.Updater;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Main;

namespace Sovereign.ClientCore.Rendering.Scenes.Update;

/// <summary>
///     Main GUI display for the autoupdater.
/// </summary>
public class UpdaterGui
{
    private readonly AutoUpdater autoUpdater;
    private readonly ClientConfigurationManager configurationManager;
    private readonly CoreController coreController;
    private readonly IEventSender eventSender;

    public UpdaterGui(AutoUpdater autoUpdater, ClientConfigurationManager configurationManager,
        CoreController coreController, IEventSender eventSender)
    {
        this.autoUpdater = autoUpdater;
        this.configurationManager = configurationManager;
        this.coreController = coreController;
        this.eventSender = eventSender;
    }

    /// <summary>
    ///     Renders the autoupdater GUI.
    /// </summary>
    public void Render()
    {
        if (!configurationManager.ClientConfiguration.AutoUpdater.UpdateOnStartup) return;
        if (!ImGui.Begin("Update")) return;

        switch (autoUpdater.State)
        {
            case AutoUpdaterState.NotStarted:
                RenderNotStarted();
                break;
            
            case AutoUpdaterState.Pending:
                RenderPending();
                break;
            
            case AutoUpdaterState.GetRelease:
                RenderGetRelease();
                break;
            
            case AutoUpdaterState.GetIndex:
                RenderGetIndex();
                break;
            
            case AutoUpdaterState.GetFile:
                RenderGetFile();
                break;
            
            case AutoUpdaterState.Complete:
                RenderComplete();
                break;
            
            case AutoUpdaterState.Error:
                RenderError();
                break;
        }

        ImGui.End();
    }

    /// <summary>
    ///     Renders the GUI in the NotStarted state.
    /// </summary>
    private void RenderNotStarted()
    {
        if (configurationManager.ClientConfiguration.AutoUpdater.PromptForUpdate)
        {
            ImGui.Text("Check for updates?");
            
            if (ImGui.Button("Yes")) autoUpdater.UpdateInBackground();
            ImGui.SameLine();
            if (ImGui.Button("No")) autoUpdater.SkipUpdates();
        }
        else
        {
            autoUpdater.UpdateInBackground();
        }
    }

    /// <summary>
    ///     Renders the GUI in the Pending state.
    /// </summary>
    private void RenderPending()
    {
        ImGui.Text("Starting update...");
    }

    /// <summary>
    ///     Renders the GUI in the GetRelease state.
    /// </summary>
    private void RenderGetRelease()
    {
        ImGui.Text("Retrieiving release information...");
    }

    /// <summary>
    ///     Renders the GUI in the GetIndex state.
    /// </summary>
    private void RenderGetIndex()
    {
        ImGui.Text("Retrieving resource index...");
    }

    /// <summary>
    ///     Renders the GUI in the GetFile state.
    /// </summary>
    private void RenderGetFile()
    {
        ImGui.Text($"Updating {autoUpdater.CurrentFile}...");
        ImGui.ProgressBar(autoUpdater.PercentComplete * 0.01f);
    }

    /// <summary>
    ///     Renders the GUI in the Complete state.
    /// </summary>
    private void RenderComplete()
    {
        ImGui.Text("Update complete.");
    }

    /// <summary>
    ///     Renders the GUI in the Error state.
    /// </summary>
    private void RenderError()
    {
        ImGui.Text("An error occurred during update.");
        ImGui.Text(autoUpdater.Error);
        
        if (ImGui.Button("Retry")) autoUpdater.UpdateInBackground();
        ImGui.SameLine();
        if (ImGui.Button("Exit")) coreController.Quit(eventSender);
    }
}