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

#version 450

layout (location = 0) in vec2 texCoord;
layout (location = 1) in vec4 color;
layout (location = 2) in vec4 shadowPosition;

layout (location = 0) out vec4 colorOut;

layout (set = 0, binding = 1) uniform texture2D g_textureAtlas;
layout (set = 0, binding = 2) uniform sampler g_textureAtlasSampler;
layout (set = 0, binding = 3) uniform texture2D g_shadowMap;
layout (set = 0, binding = 4) uniform sampler g_shadowMapSampler;
layout (set = 0, binding = 5) uniform ShaderConstants
{
    vec4 ambientLightColor; /* ambient light color; appears in shadows */
    vec4 globalLightColor;  /* global light color (e.g. sun, moon) */
} g_shaderConstants;

void main()
{
    colorOut = color * texture(sampler2D(g_textureAtlas, g_textureAtlasSampler), texCoord);
}
