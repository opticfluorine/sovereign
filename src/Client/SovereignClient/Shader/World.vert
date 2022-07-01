/*
 * Sovereign Engine
 * Copyright (c) 2022 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

#version 450

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vVelocity;
layout(location = 2) in vec2 vTexCoord;

layout(location = 0) out vec2 vTexCoordOut;

layout(binding = 0) uniform ShaderConstants
{
    mat4 g_transform;
    float g_timeSinceTick;
    float g_reserved0;
    float g_reserved1;
    float g_reserved2;
} shaderConstants;

void main()
{
    // Interpolate the position within the current game tick.
    // This assumes no acceleration is present.
    vec3 interpolated = vPosition + shaderConstants.g_timeSinceTick * vVelocity;
    
    // Embed the position in a vec4 and apply the 2D perspective transform.
    gl_Position = shaderConstants.g_transform * vec4(interpolated, 1.0);
    
    // Forward the texture coordinate for the vertex.
    vTexCoordOut = vTexCoord;
}
