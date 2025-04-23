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
layout (location = 3) in vec4 nonBlockShadowPosition;
layout (location = 4) in float vertexDepth;
layout (location = 5) in float shadowFloor;
layout (origin_upper_left) in vec4 gl_FragCoord;

layout (location = 0) out vec4 colorOut;

layout (set = 0, binding = 2) uniform texture2D g_textureAtlas;
layout (set = 0, binding = 3) uniform sampler g_textureAtlasSampler;
layout (set = 0, binding = 4) uniform texture2D g_blockShadowMap;
layout (set = 0, binding = 5) uniform sampler g_blockShadowMapSampler;
layout (set = 0, binding = 6) uniform texture2D g_nonBlockShadowMap;
layout (set = 0, binding = 7) uniform sampler g_nonBlockShadowMapSampler;
layout (set = 0, binding = 8) uniform texture2D g_lightMap;
layout (set = 0, binding = 9) uniform sampler g_lightMapSampler;
layout (set = 0, binding = 10) uniform ShaderConstants
{
    vec4 g_ambientLightColor; /* ambient light color; appears in shadows */
    vec4 g_globalLightColor;  /* global light color (e.g. sun, moon) */
    vec2 g_viewportSize;      /* viewport size in pixels */
    vec2 g_unused;            /* unused */
};

const vec3 blockShadowBias = vec3(0.0f, 0.0f, 0.001f);
const vec3 nonBlockShadowBias = vec3(0.0f, 0.0f, 0.001f);

void main()
{
    // Sample shadow map for this fragment.
    float bs = texture(sampler2DShadow(g_blockShadowMap, g_blockShadowMapSampler),
                       shadowPosition.xyz + blockShadowBias);
    float nbs = max(texture(sampler2DShadow(g_nonBlockShadowMap, g_nonBlockShadowMapSampler),
                            nonBlockShadowPosition.xyz + nonBlockShadowBias), shadowFloor);

    // Determine base lighting color from ambient and global lights.
    vec2 lightMapCoord = gl_FragCoord.xy / g_viewportSize;
    vec4 pointColor = texture(sampler2D(g_lightMap, g_lightMapSampler), lightMapCoord);
    vec4 baseColor = g_ambientLightColor + bs * nbs * (g_globalLightColor - g_ambientLightColor);
    vec4 fullColor = color * clamp(baseColor + pointColor, 0.0f, 1.0f);

    // Blend everything to a final color.
    colorOut = fullColor * texture(sampler2D(g_textureAtlas, g_textureAtlasSampler), texCoord);
    gl_FragDepth = vertexDepth;
}
