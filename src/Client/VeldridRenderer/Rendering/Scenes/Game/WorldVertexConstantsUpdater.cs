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
using System.Numerics;
using Sovereign.ClientCore.Rendering.Scenes;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for updating the world rendering vertex constant buffers.
/// </summary>
public class WorldVertexConstantsUpdater
{
    private readonly GameResourceManager gameResourceManager;

    public WorldVertexConstantsUpdater(GameResourceManager gameResourceManager)
    {
        this.gameResourceManager = gameResourceManager;
    }

    /// <summary>
    ///     Updates the constant buffers for the vertex shader for world rendering.
    /// </summary>
    /// <param name="scene">Active scene.</param>
    public void Update(IScene scene)
    {
        if (gameResourceManager.VertexUniformBuffer == null
            || gameResourceManager.BlockShadowVertexUniformBuffer == null)
            throw new InvalidOperationException("Vertex uniform buffers not ready.");

        /* Retrieve the needed constants. */
        scene.PopulateWorldVertexConstants(out var widthInTiles,
            out var heightInTiles,
            out var cameraPos,
            out var timeSinceTick,
            out var globalLightAngleRad);
        var invHalfWidth = 2.0f / widthInTiles;
        var invHalfHeight = 2.0f / heightInTiles;

        /* Update constant buffers. */
        var buf = gameResourceManager.VertexUniformBuffer.Buffer;
        var shadowBuf = gameResourceManager.BlockShadowVertexUniformBuffer.Buffer;
        buf[0].TimeSinceTick = timeSinceTick;

        // Calculate camera rotation matrix for the global light source.
        // Note that we reverse the indices to match the column-major layout expected by Vulkan.
        var rotMat = Matrix4x4.Identity;
        var sinTheta = (float)Math.Sin(globalLightAngleRad);
        var cosTheta = (float)Math.Cos(globalLightAngleRad);
        rotMat.M11 = cosTheta;
        rotMat.M21 = -sinTheta;
        rotMat.M12 = sinTheta;
        rotMat.M22 = cosTheta;

        /* Calculate world-view transform matrix. */
        ref var projMat = ref buf[0].WorldViewTransform;

        // Note that we reverse the indices to match the column-major layout expected by Vulkan.
        projMat.M11 = invHalfWidth;
        projMat.M21 = 0.0f;
        projMat.M31 = 0.0f;
        projMat.M41 = -invHalfWidth * cameraPos.X;

        // Note that for Vulkan, the Y axis is inverted.
        projMat.M12 = 0.0f;
        projMat.M22 = -invHalfHeight;
        projMat.M32 = -invHalfHeight;
        projMat.M42 = -invHalfHeight * (cameraPos.Z - cameraPos.Y);

        projMat.M13 = 0.0f;
        projMat.M23 = 0.0f;
        projMat.M33 = 0.0f;
        projMat.M43 = 0.0f;

        projMat.M14 = 0.0f;
        projMat.M24 = 0.0f;
        projMat.M34 = 0.0f;
        projMat.M44 = 1.0f;

        // Calculate the shadow map transform matrix.
        // The transform operates to the right, so the rotation needs to be the rightmost operator.
        buf[0].ShadowWorldViewTransform = projMat * rotMat;
        shadowBuf[0].WorldViewTransform = buf[0].ShadowWorldViewTransform;
    }
}