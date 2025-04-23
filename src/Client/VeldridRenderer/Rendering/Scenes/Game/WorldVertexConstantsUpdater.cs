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
using Sovereign.ClientCore.Rendering.Scenes.Game.World;
using Sovereign.VeldridRenderer.Rendering.Scenes.Game.NonBlockShadow;

namespace Sovereign.VeldridRenderer.Rendering.Scenes.Game;

/// <summary>
///     Responsible for updating the world rendering vertex constant buffers.
/// </summary>
public class WorldVertexConstantsUpdater
{
    private readonly GameResourceManager gameResourceManager;
    private readonly WorldRangeSelector rangeSelector;
    private readonly NonBlockShadowMap shadowMap;

    public WorldVertexConstantsUpdater(GameResourceManager gameResourceManager,
        WorldRangeSelector rangeSelector, NonBlockShadowMap shadowMap)
    {
        this.gameResourceManager = gameResourceManager;
        this.rangeSelector = rangeSelector;
        this.shadowMap = shadowMap;
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
            out var globalLightThetaRad,
            out var globalLightPhiRad);
        var invHalfWidth = 2.0f / widthInTiles;
        var invHalfHeight = 2.0f / heightInTiles;
        var invHeight = 1.0f / heightInTiles;
        var invWidthShadow = 1.4f / widthInTiles;
        var invHeightShadow = 0.8f / heightInTiles;

        /* Update constant buffers. */
        var buf = gameResourceManager.VertexUniformBuffer.Buffer;
        var shadowBuf = gameResourceManager.BlockShadowVertexUniformBuffer.Buffer;
        buf[0].TimeSinceTick = timeSinceTick;

        // Calculate camera rotation matrix for the global light source.
        // Note that we reverse the indices to match the column-major layout expected by Vulkan.
        var rotMat = Matrix4x4.Identity;
        var sinTheta = (float)Math.Sin(globalLightThetaRad);
        var cosTheta = (float)Math.Cos(globalLightThetaRad);
        rotMat.M11 = cosTheta;
        rotMat.M21 = -sinTheta;
        rotMat.M12 = sinTheta;
        rotMat.M22 = cosTheta;

        var rotMatPhi = Matrix4x4.Identity;
        if (globalLightPhiRad != 0.0f)
        {
            var sinPhi = (float)Math.Sin(globalLightPhiRad);
            var cosPhi = (float)Math.Cos(globalLightPhiRad);
            rotMatPhi.M22 = cosPhi;
            rotMatPhi.M23 = -sinPhi;
            rotMatPhi.M32 = sinPhi;
            rotMatPhi.M33 = cosPhi;
        }

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
        projMat.M42 = invHalfHeight * (cameraPos.Z + cameraPos.Y);

        // Clamp the visible portion of the z axis to [0, 1].
        projMat.M13 = 0.0f;
        projMat.M23 = 0.0f;
        projMat.M33 = invHeight;
        projMat.M43 = cameraPos.Z * invHeight + 0.5f;

        projMat.M14 = 0.0f;
        projMat.M24 = 0.0f;
        projMat.M34 = 0.0f;
        projMat.M44 = 1.0f;

        // Calculate the shadow map transform matrix.
        var shadowProjMat = Matrix4x4.Identity;
        shadowProjMat.M11 = invWidthShadow;
        shadowProjMat.M21 = 0.0f;
        shadowProjMat.M31 = 0.0f;
        shadowProjMat.M41 = -invWidthShadow * cameraPos.X;

        shadowProjMat.M12 = 0.0f;
        shadowProjMat.M22 = -invHeightShadow;
        shadowProjMat.M32 = -invHeightShadow;
        shadowProjMat.M42 = invHeightShadow * (cameraPos.Z + cameraPos.Y);

        shadowProjMat.M13 = 0.0f;
        shadowProjMat.M23 = 0.0f;
        shadowProjMat.M33 = invHeightShadow;
        shadowProjMat.M43 = cameraPos.Z * invHeightShadow + 0.5f;

        shadowProjMat.M14 = 0.0f;
        shadowProjMat.M24 = 0.0f;
        shadowProjMat.M34 = 0.0f;
        shadowProjMat.M44 = 1.0f;

        // NDC coordinates are [-1,1], while normalized texture coordinates are [0,1].
        // Need to shift in the world vertex shader when sampling the shadow map.
        var projToTexMat = Matrix4x4.Identity;
        projToTexMat.M11 = 0.5f;
        projToTexMat.M41 = 0.5f;
        projToTexMat.M22 = 0.5f;
        projToTexMat.M42 = 0.5f;

        // Matrices are transposed here, so treat as if acting to the left.
        shadowBuf[0].WorldViewTransform = rotMat * rotMatPhi * shadowProjMat;
        buf[0].ShadowWorldViewTransform = shadowBuf[0].WorldViewTransform * projToTexMat;

        // Draw depth is calculated along the y axis to give correct front-to-back ordering
        // of projected sprites. Need to normalize the y window to [0,1] with 1.0f being the
        // far plane of the draw volume.
        rangeSelector.DetermineExtents(out var minExtent, out var maxExtent, cameraPos);
        buf[0].YDepthScale = 1.0f / (maxExtent.Y - minExtent.Y);
        buf[0].YDepthOffset = -buf[0].YDepthScale * minExtent.Y;

        UpdateShadowMapConstants(timeSinceTick, invHalfWidth, invHalfHeight, cameraPos, maxExtent.Y - minExtent.Y);
    }

    /// <summary>
    ///     Updates the non-block shadow map shader constants.
    /// </summary>
    /// <param name="timeSinceTick">Time since last tick, in seconds.</param>
    /// <param name="invHalfWidth">1 / (screen width in world units).</param>
    /// <param name="invHalfHeight">1 / (screen height in world units).</param>
    /// <param name="cameraPos">Camera position in world units.</param>
    /// <param name="zLength">Length of the line segment along z centered on the camera position to consider.</param>
    private void UpdateShadowMapConstants(float timeSinceTick, float invHalfWidth, float invHalfHeight,
        Vector3 cameraPos, int zLength)
    {
        var buf = shadowMap.ConstantsBuffer.Value.Buffer;

        buf[0].TimeSinceTick = timeSinceTick;

        // Shadow map transform includes the camera translation but no Z-projection.
        // The camera translation includes its local Z-projection in order to align the center point
        // of the shadow map and the world render volume.
        buf[0].DrawTransform = Matrix4x4.Identity;

        buf[0].DrawTransform.M11 = invHalfWidth;
        buf[0].DrawTransform.M41 = -invHalfWidth * cameraPos.X;

        buf[0].DrawTransform.M22 = -invHalfHeight;
        buf[0].DrawTransform.M42 = invHalfHeight * (cameraPos.Y + cameraPos.Z);

        // The Z transform assigns a relative depth from a camera placed above the scene,
        // with 1.0f being the farthest (lowest Z) object.
        var invHalfZ = 2.0f / zLength;
        buf[0].DrawTransform.M33 = -invHalfZ;
        buf[0].DrawTransform.M43 = invHalfZ * cameraPos.Z + 0.5f;

        // The sample transform is the same as the draw transform, but the output coordinates
        // run over [0,1] instead of [-1,1].
        buf[0].SampleTransform = Matrix4x4.Identity;

        buf[0].SampleTransform.M11 = 0.5f * invHalfWidth;
        buf[0].SampleTransform.M41 = -0.5f * invHalfWidth * cameraPos.X + 0.5f;

        buf[0].SampleTransform.M22 = -0.5f * invHalfHeight;
        buf[0].SampleTransform.M42 = 0.5f * invHalfHeight * (cameraPos.Y + cameraPos.Z) + 0.5f;

        buf[0].SampleTransform.M33 = -invHalfZ;
        buf[0].SampleTransform.M43 = invHalfZ * cameraPos.Z + 0.5f;
    }
}