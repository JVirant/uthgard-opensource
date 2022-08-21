float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

sampler sam = sampler_state {
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct VS_INPUT
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
};


//-----------------------------------------------------------------------------
// Vertex shader output structure
//-----------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Position: POSITION;
	float2 TexCoord: TEXCOORD0;
};

VS_OUTPUT vs(VS_INPUT Input)
{
	VS_OUTPUT Output;

	float4x4 WorldView = mul(World, View);
	float4x4 WorldViewProjection = mul(WorldView, Projection);

	Output.Position = mul(Input.Position, WorldViewProjection);
	Output.TexCoord = Input.TexCoord;

	return Output;
}

float4 ps(VS_OUTPUT Input) : COLOR
{
	// return float4(Input.TexCoord.xy / 50, tex2D(sam, Input.TexCoord).b, 1);
	return tex2D(sam, Input.TexCoord.xy);
}
///////////////////////////////////////////////////////////////////////////////////////////////////////

technique PM
{
	pass P0
	{
		VertexShader = compile vs_2_0 vs();
		PixelShader = compile ps_2_0 ps();
	}
}

