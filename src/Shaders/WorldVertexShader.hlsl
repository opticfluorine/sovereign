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
    float4x4 g_transform;
    float g_timeSinceTick;
    float3 g_reserved;
};

/* Vertex shader input. */
struct VsInput {
	float3 vPosition : POSITION;
    float3 vVelocity : VELOCITY;
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

    /* Interpolate the position given the velocity. */
    float3 interpolated = input.vPosition + mul(g_timeSinceTick, input.vVelocity);

    /* Add the w coordinate and transform to device coordinates. */
    float4 original = { interpolated, 1.0f };
    output.vPosition = mul(g_transform, original);

	/* Texture coordinates are passed through. */
	output.vTexCoord = input.vTexCoord;

	return output;
}
