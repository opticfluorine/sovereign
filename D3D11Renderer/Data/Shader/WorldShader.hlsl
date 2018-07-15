/*
 * World shaders
 * Responsible for rendering single layers of the world
 *
 * Copyright (c) 2018 opticfluorine. All Rights Reserved.
 */

/* Shader constants. */
cbuffer ShaderConstants {
	float3 g_worldScale;       // x, y, z scaling factors
	float3 g_cameraPos;        // x, y, z position of camera
	float g_layerZ;            // z position of current layer
};

/* Vertex shader input. */
struct VsInput {
	float2 vPosition : POSITION;
	float2 vTexCoord : TEXCOORD;
};

/* Vertex shader output. */
struct VsOutput {
	float2 vPosition : SV_POSITION;
	float2 vTexCoord : TEXCOORD;
};

/* Single-layer world vertex shader. */
VsOutput worldVertexShader(VsInput input) {
	VsOutput output;

	/* x is offset by the camera, then scaled. */
	output.vPosition.x = g_worldScale.x * (input.vPosition.x - g_cameraPos.x);

	/* y is offset by the camera and by z, then scaled. */
	output.vPosition.y = g_worldScale.y * (input.vPosition.y - g_cameraPos.y)
		- g_worldScale.z * (g_layerZ - g_cameraPos.z);

	/* Texture coordinates are passed through. */
	output.texCoord = input.texCoord;

	return output;
}


// ======================================================================


/* Texture atlas. */
Texture2D g_textureAtlas;

/* Texture atlas sampler. */
SamplerState g_textureAtlasSampler;

/* Pixel shader input. */
struct PsInput {
	float2 pos : SV_POSITION;
	float2 texCoord : TEXCOORD;
};

/* Single-layer world pixel shader. */
float4 worldPixelShader(PsInput input) {
	float4 output;

	/* Sample the texture atlas directly. */
	output = g_textureAtlas.Sample(g_textureAtlasSampler, input.texCoord);

	return output;
};
