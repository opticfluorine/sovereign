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

namespace Sovereign.ClientCore.Rendering.Scenes.Update;

/// <summary>
///     Autoupdater scene.
/// </summary>
public class UpdateScene : IScene
{
    private readonly RenderCamera camera;
    private readonly IEngineConfiguration engineConfiguration;
    private readonly ISystemTimer systemTimer;
    private readonly UpdaterGui updaterGui;
    private readonly DisplayViewport viewport;
    private ulong systemTime;
    private float timeSinceTick;

    public UpdateScene(DisplayViewport viewport, RenderCamera camera, ISystemTimer systemTimer,
        IEngineConfiguration engineConfiguration, UpdaterGui updaterGui)
    {
        this.viewport = viewport;
        this.camera = camera;
        this.systemTimer = systemTimer;
        this.engineConfiguration = engineConfiguration;
        this.updaterGui = updaterGui;
    }

    public SceneType SceneType => SceneType.Update;
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

    public void PopulateBuffers(WorldVertex[] vertexBuffer, uint[] indexBuffer, int[] drawLengths, out int vertexCount,
        out int indexCount, out int drawCount)
    {
        vertexCount = 0;
        indexCount = 0;
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
        updaterGui.Render();
    }
}