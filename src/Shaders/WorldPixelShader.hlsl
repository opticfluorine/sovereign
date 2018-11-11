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

/* Texture atlas. */
Texture2D g_textureAtlas : register(t0);

/* Texture atlas sampler. */
SamplerState g_textureAtlasSampler : register(s0);

/* Pixel shader input. */
struct PsInput {
	float4 pos : SV_POSITION; /* only x and y are used */
	float2 texCoord : TEXCOORD;
};

struct PsOutput {
    float4 color : SV_TARGET0;
};

/* Single-layer world pixel shader. */
PsOutput main(PsInput input) {
	PsOutput output;

	/* Sample the texture atlas directly. */
	output.color = g_textureAtlas.Sample(g_textureAtlasSampler, input.texCoord);

	return output;
};
