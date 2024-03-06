/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Scenes;
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.VeldridRenderer.Rendering.Scenes.Game;

namespace Sovereign.VeldridRenderer.Rendering;

/// <summary>
///     Primary scene consumer for the Veldrid renderer.
/// </summary>
public class VeldridSceneConsumer : ISceneConsumer, IDisposable
{
    private readonly GameSceneConsumer gameSceneConsumer;
    private readonly ClientStateServices stateServices;

    /// <summary>
    ///     Dispose flag.
    /// </summary>
    private bool isDisposed;

    public VeldridSceneConsumer(GameSceneConsumer gameSceneConsumer, ClientStateServices stateServices)
    {
        this.gameSceneConsumer = gameSceneConsumer;
        this.stateServices = stateServices;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            gameSceneConsumer.Dispose();
            isDisposed = true;
        }
    }

    public void ConsumeScene(IScene scene)
    {
        /* Dispatch to scene-specific consumers. */
        switch (scene.SceneType)
        {
            case SceneType.Game:
                gameSceneConsumer.ConsumeScene(scene);
                break;
        }

        // General processing.
        if (scene.RenderGui)
        {
            // Global debug menus.
            if (stateServices.GetStateFlagValue(ClientStateFlag.ShowImGuiMetrics)) ImGui.ShowMetricsWindow();
            if (stateServices.GetStateFlagValue(ClientStateFlag.ShowImGuiDebugLog)) ImGui.ShowDebugLogWindow();

            // State specific updates.
            scene.UpdateGui();
        }
    }

    /// <summary>
    ///     Initializes the scene tree.
    /// </summary>
    public void Initialize()
    {
        gameSceneConsumer.Initialize();
    }
}