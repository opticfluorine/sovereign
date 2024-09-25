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
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Scenes.Game.Gui;
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.EngineCore.Configuration;
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
    private readonly ISystemTimer systemTimer;
    private readonly DisplayViewport viewport;
    private readonly WorldVertexSequencer worldVertexSequencer;

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
        RenderCamera camera, DisplayViewport viewport, MainDisplay mainDisplay,
        WorldVertexSequencer worldVertexSequencer, GameGui gameGui)
    {
        this.systemTimer = systemTimer;
        this.engineConfiguration = engineConfiguration;
        this.camera = camera;
        this.viewport = viewport;
        this.worldVertexSequencer = worldVertexSequencer;
        this.gameGui = gameGui;
    }

    public SceneType SceneType => SceneType.Game;

    public bool RenderGui => true;

    public void BeginScene()
    {
        ComputeTimes();
    }

    public void EndScene()
    {
    }

    public void BuildRenderPlan(RenderPlan renderPlan)
    {
        // Start by scheduling the shadow map render pass.
        renderPlan.PushDebugGroup("Shadow Map");
        renderPlan.DrawBlockShadowMap();
        renderPlan.PopDebugGroup();

        // Fill the buffers and generate sprite draw commands along the way.
        worldVertexSequencer.SequenceVertices(renderPlan, timeSinceTick, systemTime);
    }

    public void PopulateWorldVertexConstants(out float widthInTiles, out float heightInTiles,
        out Vector3 cameraPos, out float timeSinceTick)
    {
        widthInTiles = viewport.WidthInTiles;
        heightInTiles = viewport.HeightInTiles;
        cameraPos = camera.Aim(this.timeSinceTick);
        timeSinceTick = this.timeSinceTick;
    }

    public void UpdateGui()
    {
        gameGui.Render();
    }

    /// <summary>
    ///     Updates systemTime and timeSinceTick.
    /// </summary>
    private void ComputeTimes()
    {
        systemTime = systemTimer.GetTime();
        timeSinceTick = systemTime % engineConfiguration.EventTickInterval
                        * UnitConversions.UsToS;
    }
}