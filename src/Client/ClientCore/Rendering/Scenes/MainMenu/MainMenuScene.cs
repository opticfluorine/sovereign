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

using System.Numerics;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     Scene for the main menu displayed at startup.
/// </summary>
public class MainMenuScene : IScene
{
    private readonly RenderCamera camera;
    private readonly CreatePlayerGui createPlayerGui;
    private readonly IEngineConfiguration engineConfiguration;
    private readonly LoginGui loginGui;
    private readonly PlayerSelectionGui playerSelectionGui;
    private readonly RegistrationGui registrationGui;
    private readonly StartupGui startupGui;

    private readonly ISystemTimer systemTimer;
    private readonly DisplayViewport viewport;

    /// <summary>
    ///     Previous internal state.
    /// </summary>
    private MainMenuState lastState = MainMenuState.Login;

    /// <summary>
    ///     Current internal state.
    /// </summary>
    private MainMenuState state = MainMenuState.Startup;

    /// <summary>
    ///     System time of the current frame, in microseconds.
    /// </summary>
    private ulong systemTime;

    /// <summary>
    ///     Time since the current tick started, in seconds.
    /// </summary>
    /// Evaluated at the start of rendering.
    private float timeSinceTick;

    public MainMenuScene(RenderCamera camera, DisplayViewport viewport, ISystemTimer systemTimer,
        IEngineConfiguration engineConfiguration, StartupGui startupGui, LoginGui loginGui,
        RegistrationGui registrationGui, PlayerSelectionGui playerSelectionGui, CreatePlayerGui createPlayerGui)
    {
        this.camera = camera;
        this.viewport = viewport;
        this.systemTimer = systemTimer;
        this.engineConfiguration = engineConfiguration;
        this.startupGui = startupGui;
        this.loginGui = loginGui;
        this.registrationGui = registrationGui;
        this.playerSelectionGui = playerSelectionGui;
        this.createPlayerGui = createPlayerGui;
    }

    public SceneType SceneType => SceneType.MainMenu;
    public bool RenderGui => true;

    public void BeginScene()
    {
        systemTime = systemTimer.GetTime();
        timeSinceTick = systemTime % engineConfiguration.EventTickInterval
                        * UnitConversions.UsToS;
    }

    public void EndScene()
    {
    }

    public void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer, int[] drawLengths, out int drawCount)
    {
        drawCount = 0;
    }

    public void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles, out Vector3 cameraPos,
        out float timeSinceTick)
    {
        widthInTiles = viewport.WidthInTiles;
        heightInTiles = viewport.HeightInTiles;
        cameraPos = camera.Aim(this.timeSinceTick);
        timeSinceTick = this.timeSinceTick;
    }

    public void UpdateGui()
    {
        var needToInit = lastState != state;
        lastState = state;
        switch (state)
        {
            case MainMenuState.Startup:
                state = startupGui.Render();
                break;

            case MainMenuState.Login:
                if (needToInit) loginGui.Initialize();
                state = loginGui.Render();
                break;

            case MainMenuState.Registration:
                if (needToInit) registrationGui.Initialize();
                state = registrationGui.Render();
                break;

            case MainMenuState.PlayerSelection:
                if (needToInit) playerSelectionGui.Initialize();
                state = playerSelectionGui.Render();
                break;

            case MainMenuState.PlayerCreation:
                if (needToInit) createPlayerGui.Initialize();
                state = createPlayerGui.Render();
                break;
        }
    }
}