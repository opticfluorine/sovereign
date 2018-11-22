/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
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

/* Shader constants. */
cbuffer ShaderConstants : register(b0)
{
	float3 g_worldScale;       // x, y, z scaling factors
	float3 g_cameraPos;        // x, y, z position of camera
    float2 g_displaySize;      // width, height of display mode
};

/* Vertex shader input. */
struct VsInput {
	float3 vPosition : POSITION;
	float2 vTexCoord : TEXCOORD;
};

/* Vertex shader output. */
struct VsOutput {
	float4 vPosition : SV_POSITION;
	float2 vTexCoord : TEXCOORD;
};

/* Single-layer world vertex shader. */
VsOutput main(VsInput input) {
	VsOutput output;

	/* x is offset by the camera, then scaled. */
	output.vPosition.x = (g_worldScale.x * (input.vPosition.x - g_cameraPos.x))
        + (g_displaySize.x * 0.5);

	/* y is offset by the camera and by z, then scaled. */
    output.vPosition.y = (g_worldScale.y * (input.vPosition.y - g_cameraPos.y)
        - g_worldScale.z * (input.vPosition.z - g_cameraPos.z))
        + (g_displaySize.y * 0.5);

    output.vPosition.z = 0.0;
    output.vPosition.w = 0.0;

	/* Texture coordinates are passed through. */
	output.vTexCoord = input.vTexCoord;

	return output;
}
