float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float4 Light1 = float4(5, 5, 5, 1.1);
float4 Light2 = float4(-5, -5, 5, 0.25);

sampler sam0 = sampler_state {
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler sam1 = sampler_state {
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
	float2 TexCoord0 : TEXCOORD0;
	float2 TexCoord1 : TEXCOORD1;
};

struct VS_OUTPUT
{
	float4 Position: POSITION;
	float2 TexCoord0: TEXCOORD0;
	float2 TexCoord1: TEXCOORD1;
	float3 Normal: TEXCOORD2;
	float4 DepthPosition: TEXCOORD3;
};

struct PS_OUTPUT
{
	float4 Color: COLOR;
	float Depth: DEPTH;
};

VS_OUTPUT vs(VS_INPUT input)
{
	VS_OUTPUT output;

	float4x4 WorldView = mul(World, View);
	float4x4 WorldViewProjection = mul(WorldView, Projection);

	output.Position = mul(input.Position, WorldViewProjection);
	output.TexCoord0 = input.TexCoord0;
	output.TexCoord1 = input.TexCoord1;
	output.Normal = normalize(mul(input.Normal, World));
	output.DepthPosition = output.Position;

	return output;
}

PS_OUTPUT ps(VS_OUTPUT input)
{
	PS_OUTPUT output;

	float4 diffuse0 = tex2D(sam0, input.TexCoord0.xy);
	float4 diffuse1 = tex2D(sam1, input.TexCoord1.xy);
	float light = saturate(dot(normalize(Light1.xyz), input.Normal)) * Light1.w;
	light += saturate(dot(normalize(Light2.xyz), input.Normal)) * Light2.w;

	output.Color = float4(diffuse0.rgb * light * diffuse0.a, diffuse0.a);
	output.Depth = diffuse0.a < 0.1 ? 1e9 : input.DepthPosition.z / input.DepthPosition.w;

	return output;
}

technique PM
{
	pass P0
	{
		VertexShader = compile vs_2_0 vs();
		PixelShader = compile ps_3_0 ps();
	}
}

