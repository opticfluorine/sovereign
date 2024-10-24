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

layout (binding = 0) uniform PointLightShaderConstants
{
    mat4 g_lightTransform;  // Model-view-projection matrix for light map.
    vec3 g_lightPosition;   // Light position in world coordinates.
    vec3 g_lightColor;      // Light color.
    float g_lightRadius;    // Light radius in world coordinates.
    float g_lightIntensity; // Light intensity.
};

const float r2_limit = 0.01f;  // cutoff point for small distances

void main() {
    float radius2 = g_lightRadius * g_lightRadius;
    float normR2 = dot(distanceFromLight, distanceFromLight) / radius2;
    float invr2 = 1.0f / max(normR2, r2_limit);
}