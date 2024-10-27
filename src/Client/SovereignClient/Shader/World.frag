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
layout (origin_upper_left) in vec4 gl_FragCoord;

layout (location = 0) out vec4 colorOut;

layout (set = 0, binding = 1) uniform texture2D g_textureAtlas;
layout (set = 0, binding = 2) uniform sampler g_textureAtlasSampler;
layout (set = 0, binding = 3) uniform texture2D g_shadowMap;
layout (set = 0, binding = 4) uniform sampler g_shadowMapSampler;
layout (set = 0, binding = 5) uniform texture2D g_lightMap;
layout (set = 0, binding = 6) uniform sampler g_lightMapSampler;
layout (set = 0, binding = 7) uniform ShaderConstants
{
    vec4 g_ambientLightColor; /* ambient light color; appears in shadows */
    vec4 g_globalLightColor;  /* global light color (e.g. sun, moon) */
    vec2 g_viewportSize;      /* viewport size in pixels */
    vec2 g_unused;            /* unused */
};

void main()
{
    // Sample shadow map for this fragment.
    vec2 shadowTexCoord = shadowPosition.xy;
    float shadowDepth = texture(sampler2D(g_shadowMap, g_shadowMapSampler), shadowTexCoord).r;

    // Determine base lighting color from ambient and global lights.
    vec2 lightMapCoord = gl_FragCoord.xy / g_viewportSize;
    vec4 pointColor = texture(sampler2D(g_lightMap, g_lightMapSampler), lightMapCoord);
    vec4 baseColor = shadowPosition.z >= shadowDepth - 0.01f ? g_globalLightColor : g_ambientLightColor;
    vec4 fullColor = color * (baseColor + pointColor);

    // Blend everything to a final color.
    colorOut = fullColor * texture(sampler2D(g_textureAtlas, g_textureAtlasSampler), texCoord);
}
