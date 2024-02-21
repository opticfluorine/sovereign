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
using ImGuiNET;
using Sovereign.ClientCore.Rendering.Configuration;
using Sovereign.ClientCore.Rendering.Display;
using Sovereign.ClientCore.Rendering.Resources.Buffers;
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
    private readonly GameSceneCamera camera;
    private readonly IEngineConfiguration engineConfiguration;
    private readonly MainDisplay mainDisplay;
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
        GameSceneCamera camera, DisplayViewport viewport, MainDisplay mainDisplay,
        WorldVertexSequencer worldVertexSequencer)
    {
        this.systemTimer = systemTimer;
        this.engineConfiguration = engineConfiguration;
        this.camera = camera;
        this.viewport = viewport;
        this.mainDisplay = mainDisplay;
        this.worldVertexSequencer = worldVertexSequencer;
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

    public void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer,
        int[] drawLengths, out int drawCount)
    {
        worldVertexSequencer.SequenceVertices(vertexBuffer, indexBuffer, drawLengths,
            out drawCount, timeSinceTick, systemTime);
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
        ImGui.ShowDemoWindow();
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