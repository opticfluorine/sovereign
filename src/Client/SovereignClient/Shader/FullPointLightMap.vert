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

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 velocity;
layout (location = 2) in vec3 texCoord;
layout (location = 3) in float lightFactor;

layout (location = 0) out vec3 distanceFromLight;
layout (location = 1) out float lightFactor_out;

layout (binding = 0) uniform PointLightShaderConstants
{
    mat4 g_lightTransform;  // Model-view-projection matrix for light map.
    vec3 g_lightPosition;   // Light position in world coordinates.
    float g_lightRadius;    // Light radius in world coordinates.
    vec3 g_lightColor;      // Light color.
    float g_lightIntensity; // Light intensity.
};

layout (binding = 1) uniform WorldVertexShaderConstants
{
    mat4 g_transform;
    mat4 g_shadowMapTransform;
    float g_timeSinceTick;
    float g_reserved0;
    float g_reserved1;
    float g_reserved2;
};

// Additive position bias used to avoid sampling self-shadows
// in the shadow maps.
const vec3 positionBias = vec3(0.00f, 0.00f, 0.01f);

void main() {
    vec3 interpolated = position + positionBias + g_timeSinceTick * velocity;
    distanceFromLight = interpolated - g_lightPosition;
    lightFactor_out = lightFactor;
    gl_Position = g_lightTransform * vec4(interpolated, 1.0f);
}