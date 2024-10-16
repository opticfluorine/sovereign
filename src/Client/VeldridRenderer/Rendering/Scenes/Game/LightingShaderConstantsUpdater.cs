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

using Sovereign.ClientCore.Rendering;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for updating shader constants for light sources.
/// </summary>
public class LightingShaderConstantsUpdater
{
    private readonly GameResourceManager gameResMgr;

    public LightingShaderConstantsUpdater(GameResourceManager gameResMgr)
    {
        this.gameResMgr = gameResMgr;
    }

    /// <summary>
    ///     Updates shader constants for point lighting based on the given render plan.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    public void UpdateConstats(RenderPlan renderPlan)
    {
        for (var i = 0; i < renderPlan.LightCount; ++i)
        {
            UpdateDepthMapConstants(ref gameResMgr.PointLightDepthMapUniformBuffer!.Buffer[i],
                ref renderPlan.Lights[i]);
        }
    }

    /// <summary>
    ///     Updates shader constants for point light depth map.
    /// </summary>
    /// <param name="constants">Shader constants to update.</param>
    /// <param name="light">Point light.</param>
    private void UpdateDepthMapConstants(ref PointLightDepthMapShaderConstants constants, ref RenderLight light)
    {
        constants.LightPosition = light.Light.Position;
        constants.RadiusSquared = light.Light.Details.Radius * light.Light.Details.Radius;
    }
}