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

layout (location = 0) out vec3 distanceFromLight;

layout (binding = 0) uniform ShaderConstants
{
    vec3 g_lightPosition;
    float g_lightRadiusSq;
    vec3 g_reserved;
} shaderConstants;

const float minL2 = 0.01;

void main() {
    distanceFromLight = position - shaderConstants.g_lightPosition;

    // Find the z-normalized basis vector which is parallel to the vector
    // from the light to the vertex. The (x,y) coordinates of this vector
    // characterize a line extending from the light source to its radius.
    // This is a nonlinear mapping of the triangle onto a sphere of unit
    // radius centered on the light source; however, the perspective-correct
    // interpolation will effectively ignore the depth information, instead
    // (correctly) interpolating over a set of rays from the light which vary
    // in density (but all of which intersect the triangle).
    //
    // The depth will be restored in the fragment shader from the above
    // distance output.
    vec3 r2 = distanceFromLight * distanceFromLight;
    float l2 = r2.x + r2.y + r2.z;
    vec3 norm = l2 >= minL2 ? normalize(distanceFromLight) : vec3(0.0f, 0.0f, 1.0f);
    vec3 ray = norm / norm.z;

    gl_Position = vec4(ray, 1.0f);
}