/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

using System.Numerics;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.EngineCore.Configuration;
using Sovereign.EngineCore.Systems.Time;
using Sovereign.EngineCore.Timing;
using Sovereign.EngineUtil.Numerics;

namespace Sovereign.ClientCore.Rendering.Scenes.Game;

/// <summary>
///     In-game scene.
/// </summary>
public sealed class GameScene : IScene
{
    private readonly RenderCamera camera;
    private readonly IEngineConfiguration engineConfiguration;
    private readonly GameGui gameGui;
    private readonly GlobalLightTable globalLightTable;
    private readonly ISystemTimer systemTimer;
    private readonly ITimeServices timeServices;
    private readonly DisplayViewport viewport;
    private readonly WorldVertexSequencer worldVertexSequencer;

    /// <summary>
    ///     Camera position for frame.
    /// </summary>
    private Vector3 cameraPos = Vector3.Zero;

    /// <summary>
    ///     System time of the current frame, in microseconds.
    /// </summary>
    private ulong systemTime;

    /// <summary>
    ///     Time since the current tick started, in seconds.
    /// </summary>
    /// Evaluated at the start of rendering.
    private float timeSinceTick;

    public GameScene(ISystemTimer systemTimer, IEngineConfiguration engineConfiguration,
        RenderCamera camera, DisplayViewport viewport, ITimeServices timeServices,
        WorldVertexSequencer worldVertexSequencer, GameGui gameGui, GlobalLightTable globalLightTable)
    {
        this.systemTimer = systemTimer;
        this.engineConfiguration = engineConfiguration;
        this.camera = camera;
        this.viewport = viewport;
        this.timeServices = timeServices;
        this.worldVertexSequencer = worldVertexSequencer;
        this.gameGui = gameGui;
        this.globalLightTable = globalLightTable;
    }

    public SceneType SceneType => SceneType.Game;

    public bool RenderGui => true;

    public void BeginScene()
    {
        ComputeTimes();
        cameraPos = camera.Aim(timeSinceTick);
    }

    public void EndScene()
    {
    }

    public void BuildRenderPlan(RenderPlan renderPlan)
    {
        // Global scene configuration.
        renderPlan.CameraPosition = cameraPos;

        // Start by scheduling the shadow map render pass.
        renderPlan.PushDebugGroup("Shadow Maps");
        renderPlan.DrawBlockShadowMap();
        renderPlan.DrawNonBlockShadowMap();
        renderPlan.DrawPointLightShadowMaps();
        renderPlan.PopDebugGroup();

        // Fill the buffers and generate sprite draw commands along the way.
        worldVertexSequencer.SequenceVertices(renderPlan, timeSinceTick, systemTime);
    }

    public void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles,
        out Vector3 cameraPos, out float timeSinceTick, out float globalLightThetaRad, out float globalLightPhiRad)
    {
        widthInTiles = viewport.WidthInTiles;
        heightInTiles = viewport.HeightInTiles;
        cameraPos = this.cameraPos;
        timeSinceTick = this.timeSinceTick;
        globalLightThetaRad = -0.1f;
        globalLightPhiRad = 0.00f;
    }

    public void PopulateWorldFragmentConstants(out Vector4 ambientLightColor, out Vector4 globalLightColor)
    {
        ambientLightColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        globalLightColor = globalLightTable.GetGlobalLightColor(timeServices.SecondOfDay);
    }

    public void UpdateGui(RenderPlan renderPlan)
    {
        gameGui.Render(renderPlan);
    }

    /// <summary>
    ///     Updates systemTime and timeSinceTick.
    /// </summary>
    private void ComputeTimes()
    {
        systemTime = systemTimer.GetTime();
        timeSinceTick = systemTime % EngineConstants.TickIntervalUs
                        * UnitConversions.UsToS;
    }
}