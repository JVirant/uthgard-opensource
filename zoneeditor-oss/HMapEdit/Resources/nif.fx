float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float4 CameraPosition;
float ObjectId;

float4 Light1 = float4(5, 5, 5, 1.1);
float3 Light1Color = float3(1, 1, 1);
float4 Light2 = float4(-5, -5, 5, 0.25);
float3 Light2Color = float3(0, 0, 1);

float4 objectColor = float4(0, 0, 0, 0);

bool hasBase = false;
int BaseTexUVSetIndex = 0;
texture BaseTexture;
sampler BaseSampler = sampler_state {
	Texture = <BaseTexture>;
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

bool hasDetail = false;
int DetailTexUVSetIndex = 1;
texture DetailTexture;
sampler DetailSampler = sampler_state {
	Texture = <DetailTexture>;
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

bool hasDark = false;
int DarkTexUVSetIndex = 2;
texture DarkTexture;
sampler DarkSampler = sampler_state {
	Texture = <DarkTexture>;
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

bool hasBump = false;
int BumpTexUVSetIndex = 0;
texture BumpTexture;
sampler BumpSampler = sampler_state {
	Texture = <BumpTexture>;
	minfilter = Anisotropic;
	mipfilter = Anisotropic;
	magfilter = Anisotropic;
	AddressU = WRAP;
	AddressV = WRAP;
};

bool hasGloss = false;
int GlossTexUVSetIndex = 0;
texture GlossTexture;
sampler GlossSampler = sampler_state {
	Texture = <GlossTexture>;
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
	float2 TexCoord[4] : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 Position: POSITION;
	float2 TexCoord[4]: TEXCOORD0;
	float3 Normal: TEXCOORD4;
	float4 DepthPosition: TEXCOORD5;
};

struct PS_OUTPUT
{
	float4 Color: COLOR0;
	float Depth: DEPTH;
	float4 ObjectId : COLOR1;
};

float3 expand(float3 v)
{
	return (v - 0.5) * 2;
}

VS_OUTPUT vs(VS_INPUT input)
{
	VS_OUTPUT output;

	float4x4 WorldView = mul(World, View);
	float4x4 WorldViewProjection = mul(WorldView, Projection);

	output.Position = mul(input.Position, WorldViewProjection);
	for (int i = 0; i < 4; ++i)
		output.TexCoord[i] = input.TexCoord[i];
	output.Normal = normalize(mul(float4(input.Normal.xyz, 1), World)).xyz;
	output.DepthPosition = output.Position;

	return output;
}

float3 calcLight(float3 viewDir, float3 normal, float4 pos, float3 color, float3 specularColor, float specularStrength, float specularGlossiness)
{
	float3 halfVec = normalize(pos); // normalize(pos.xyz - viewDir);
	float nDotH = saturate(dot(halfVec, normal));

	float3 specular = clamp(specularColor * specularStrength * pow(nDotH, specularGlossiness), 0.0, 1.0);
	specular *= color;
	return float3(nDotH, nDotH, nDotH) + specular;
}

PS_OUTPUT ps(VS_OUTPUT input)
{
	PS_OUTPUT output;

	float3 normal = input.Normal;
	float3 viewDir = normalize(CameraPosition.xyz - input.DepthPosition.xyz);

	if (hasBump)
	{
		float3 bumpMap = expand(tex2D(BumpSampler, input.TexCoord[BumpTexUVSetIndex].xy).xyz);
		normal *= bumpMap;
	}

	float3 light1halfVec = normalize(Light1.xyz - viewDir);
	float light1nDotH = saturate(dot(light1halfVec, normal));

	float3 specularColor = float3(1, 1, 1);
	float specularStrength = 1;
	float specularGlossiness = .5f;
	if (hasGloss)
		specularGlossiness = tex2D(GlossSampler, input.TexCoord[GlossTexUVSetIndex].xy).x;

	
	float3 light = calcLight(viewDir, normal, Light1, Light1Color, specularColor, specularStrength, specularGlossiness);
	light += calcLight(viewDir, normal, Light2, Light2Color, specularColor, specularStrength, specularGlossiness);

	float4 color = float4(light, 1);
	if (hasBase)
		color *= tex2D(BaseSampler, input.TexCoord[BaseTexUVSetIndex].xy);
	if (hasDark)
		color *= tex2D(DarkSampler, input.TexCoord[DarkTexUVSetIndex].xy);
	if (hasDetail)
		color *= tex2D(DetailSampler, input.TexCoord[DetailTexUVSetIndex].xy) * 2.0;

	output.Color = color * (1 - objectColor.a) + objectColor * objectColor.a;
	output.Depth = color.a < 0.1 ? 1e9 : input.DepthPosition.z / input.DepthPosition.w;
	if (color.a > 0.1)
		output.ObjectId = float4(ObjectId, ObjectId, ObjectId, 1);

	return output;
}

technique PM
{
	pass P0
	{
		VertexShader = compile vs_3_0 vs();
		PixelShader = compile ps_3_0 ps();
	}
}

