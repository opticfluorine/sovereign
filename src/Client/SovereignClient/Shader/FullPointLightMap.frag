/*
 * Sovereign Engine
 * Copyright (c) 2024 opticfluorine
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

#version 450

layout (location = 0) in vec3 distanceFromLight;
layout (location = 1) flat in float lightFactor;

layout (location = 0) out vec4 colorOut;

layout (binding = 0) uniform PointLightShaderConstants
{
    mat4 g_lightTransform;  // Model-view-projection matrix for light map.
    vec3 g_lightPosition;   // Light position in world coordinates.
    float g_lightRadius;    // Light radius in world coordinates.
    vec3 g_lightColor;      // Light color.
    float g_lightIntensity; // Light intensity.
};

layout (binding = 2) uniform textureCube g_depthMap; // Depth map (cubemap).
layout (binding = 3) uniform sampler g_sampler;      // Depth map sampler.

const float r2_limit = 0.001f;     // cutoff point for small distances
const float depthBias = 0.99f;     // depth bias for shadow map sampling

void main() {
    // Use a smoothstep instead of inverse-square because it looks better visually
    // (despite being less physically accurate).
    float radius2 = g_lightRadius * g_lightRadius;
    float r2 = dot(distanceFromLight, distanceFromLight);
    float normR2 = r2 / radius2;
    float scale = smoothstep(radius2, 0.0f, r2);

    // Check depth map for this light and fragment.
    float d = texture(samplerCubeShadow(g_depthMap, g_sampler),
                      vec4(distanceFromLight.xyz, depthBias * normR2));

    colorOut = lightFactor * vec4(d * scale * g_lightIntensity * g_lightColor, 1.0f);
}