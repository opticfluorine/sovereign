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
layout (location = 2) in vec2 texCoord;
layout (location = 3) in float lightFactor;

layout (location = 0) out vec3 distanceFromLight;

layout (binding = 0) uniform PointLightShaderConstants
{
    mat4 g_lightTransform;  // Model-view-projection matrix for light map.
    vec3 g_lightPosition;   // Light position in world coordinates.
    float g_lightRadius;    // Light radius in world coordinates.
    vec3 g_lightColor;      // Light color.
    float g_lightIntensity; // Light intensity.
};

layout (binding = 1) uniform PointLightDepthMapShaderConstants
{
    mat4 g_transform;      // Transformation matrix for depth map.
};

void main() {
    distanceFromLight = position - g_lightPosition;
    gl_Position = g_transform * vec4(position, 1.0f);
}