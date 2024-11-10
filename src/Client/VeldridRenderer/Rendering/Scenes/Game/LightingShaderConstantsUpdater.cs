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
using Sovereign.ClientCore.Rendering;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for updating shader constants for light sources.
/// </summary>
public class LightingShaderConstantsUpdater
{
    /// <summary>
    ///     Transformation matrices that rotate the camera to give the correct orientation
    ///     for each face of the cubemap.
    /// </summary>
    private static readonly Matrix4x4[] RotationMatrices =
    {
        // +X (0)
        new(0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            -1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f),

        // -X (1)
        new(0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f),

        // +Y (2)
        new(1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f),

        // -Y (3)
        new(1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f),

        // +Z (4)
        new(1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f),

        // -Z (5)
        new(-1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, -1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f)
    };

    private readonly GameResourceManager gameResMgr;

    public LightingShaderConstantsUpdater(GameResourceManager gameResMgr)
    {
        this.gameResMgr = gameResMgr;
    }

    /// <summary>
    ///     Updates shader constants for point lighting based on the given render plan.
    /// </summary>
    /// <param name="renderPlan">Render plan.</param>
    public void UpdateConstants(RenderPlan renderPlan)
    {
        for (var i = 0; i < renderPlan.LightCount; ++i) UpdateConstantsForLight(i, renderPlan);
    }

    /// <summary>
    ///     Updates the transformation matrices for each cube face for the given light.
    /// </summary>
    /// <param name="index">Light index.</param>
    /// <param name="renderPlan">Render plan.</param>
    private void UpdateConstantsForLight(int index, RenderPlan renderPlan)
    {
        ref var light = ref renderPlan.Lights[index];
        var radius = light.Light.Details.Radius;
        var pos = light.Light.Position;
        gameResMgr.PointLightBuffer![index] = new PointLightShaderConstants
        {
            LightPosition = light.Light.Position,
            Radius = light.Light.Details.Radius,
            Intensity = light.Light.Details.Intensity,
            LightColor = light.Light.Details.Color,
            LightTransform = new Matrix4x4(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.5f * (1.0f + radius), 0.0f,
                -pos.X, pos.Y + pos.Z, 0.5f * (1.0f - pos.Z), radius)
        };

        // World-projection matrix shared by all cube faces.
        var invRadius = 1.0f / light.Light.Details.Radius;
        var worldProjection = new Matrix4x4(
            invRadius, 0.0f, 0.0f, 0.0f,
            0.0f, -invRadius, 0.0f, 0.0f,
            0.0f, 0.0f, invRadius, invRadius,
            0.0f, 0.0f, 0.0f, 0.0f
        );

        // Camera translation matrix shared by all cube faces.
        var cameraTranslation = new Matrix4x4(
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            -light.Light.Position.X, -light.Light.Position.Y, -light.Light.Position.Z, 1.0f
        );

        // Update for each face.
        for (var i = 0; i < PointLightDepthMap.LayerCount; ++i)
            gameResMgr.PointLightDepthMapBuffer![(int)PointLightDepthMap.LayerCount * index + i] =
                new PointLightDepthMapShaderConstants
                {
                    Transform = cameraTranslation * RotationMatrices[i] * worldProjection
                };
    }
}