float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 Light = float3(5, 5, 5);

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
};

VS_OUTPUT vs(VS_INPUT Input)
{
	VS_OUTPUT Output;

	float4x4 WorldView = mul(World, View);
	float4x4 WorldViewProjection = mul(WorldView, Projection);

	Output.Position = mul(Input.Position, WorldViewProjection);
	Output.TexCoord0 = Input.TexCoord0;
	Output.TexCoord1 = Input.TexCoord1;
	Output.Normal = normalize(mul(Input.Normal, World));

	return Output;
}

float4 ps(VS_OUTPUT Input) : COLOR
{
	float4 diffuse0 = tex2D(sam0, Input.TexCoord0.xy);
	float4 diffuse1 = tex2D(sam1, Input.TexCoord1.xy);
	float light = (0.1 + saturate(dot(normalize(Light), Input.Normal)));
	return float4(diffuse0.rgb * light * diffuse0.a, diffuse0.a);
}

technique PM
{
	pass P0
	{
		VertexShader = compile vs_2_0 vs();
		PixelShader = compile ps_2_0 ps();
	}
}

