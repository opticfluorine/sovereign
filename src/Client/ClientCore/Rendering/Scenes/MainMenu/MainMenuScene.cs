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
using Sovereign.ClientCore.Systems.ClientState;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Events;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.MainMenu;

/// <summary>
///     Scene for the main menu displayed at startup.
/// </summary>
public class MainMenuScene : IScene
{
    private readonly RenderCamera camera;
    private readonly ConnectionLostGui connectionLostGui;
    private readonly CreatePlayerGui createPlayerGui;
    private readonly IEngineConfiguration engineConfiguration;
    private readonly IEventSender eventSender;
    private readonly LoginGui loginGui;
    private readonly PlayerSelectionGui playerSelectionGui;
    private readonly RegistrationGui registrationGui;
    private readonly StartupGui startupGui;
    private readonly ClientStateController stateController;
    private readonly ClientStateServices stateServices;

    private readonly ISystemTimer systemTimer;
    private readonly DisplayViewport viewport;

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
        RegistrationGui registrationGui, PlayerSelectionGui playerSelectionGui, CreatePlayerGui createPlayerGui,
        IEventSender eventSender, ClientStateController stateController, ClientStateServices stateServices,
        ConnectionLostGui connectionLostGui)
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
        this.eventSender = eventSender;
        this.stateController = stateController;
        this.stateServices = stateServices;
        this.connectionLostGui = connectionLostGui;
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

    public void BuildRenderPlan(RenderPlan renderPlan)
    {
    }

    public void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles, out Vector3 cameraPos,
        out float timeSinceTick, out float globalLightAngleRad)
    {
        widthInTiles = viewport.WidthInTiles;
        heightInTiles = viewport.HeightInTiles;
        cameraPos = camera.Aim(this.timeSinceTick);
        timeSinceTick = this.timeSinceTick;
        globalLightAngleRad = 0.0f;
    }

    public void PopulateWorldFragmentConstants(out Vector4 ambientLightColor, out Vector4 globalLightColor)
    {
        ambientLightColor = Vector4.One;
        globalLightColor = Vector4.One;
    }

    public void UpdateGui()
    {
        var needToInit = stateServices.CheckAndClearMainMenuResetFlag();
        var lastState = stateServices.MainMenuState;
        var newState = lastState;
        switch (stateServices.MainMenuState)
        {
            case MainMenuState.Startup:
                newState = startupGui.Render();
                break;

            case MainMenuState.Login:
                if (needToInit) loginGui.Initialize();
                newState = loginGui.Render();
                break;

            case MainMenuState.Registration:
                if (needToInit) registrationGui.Initialize();
                newState = registrationGui.Render();
                break;

            case MainMenuState.PlayerSelection:
                if (needToInit) playerSelectionGui.Initialize();
                newState = playerSelectionGui.Render();
                break;

            case MainMenuState.PlayerCreation:
                if (needToInit) createPlayerGui.Initialize();
                newState = createPlayerGui.Render();
                break;

            case MainMenuState.ConnectionLost:
                newState = connectionLostGui.Render();
                break;
        }

        if (newState != lastState)
            stateController.SetMainMenuState(eventSender, newState);
    }
}