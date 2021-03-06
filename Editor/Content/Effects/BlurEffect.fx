// Blurs a texture using poisson disk sampling. Uses the depth map 
// to make sure blur does not bleed between objects that are far apart

#include "Includes/Defines.hlsl"
#include "Includes/GBuffer.hlsl"

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float MaxDistance;
float MaxBlurDistance = 0.25f;
float SampleRadius = 8.0f;

Texture2D SourceMap;
sampler sourceSampler = sampler_state
{
    Texture = (SourceMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

static float2 Offsets[16] =
{
    float2(0.2770745f, 0.6951455f),
    float2(0.1874257f, -0.02561589f),
    float2(-0.3381929f, 0.8713168f),
    float2(0.5867746f, 0.1087471f),
    float2(-0.3078699f, 0.188545f),
    float2(0.7993396f, 0.4595091f),
    float2(-0.09242552f, 0.5260149f),
    float2(0.3657553f, -0.5329605f),
    float2(-0.3829718f, -0.2476171f),
    float2(-0.01085108f, -0.6966301f),
    float2(0.8404155f, -0.3543923f),
    float2(-0.5186161f, -0.7624033f),
    float2(-0.8135794f, 0.2328489f),
    float2(-0.784665f, -0.2434929f),
    float2(0.9920505f, 0.0855163f),
    float2(-0.687256f, 0.6711345f)
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{    
    float2 texCoord = input.TexCoord;
    float2 mapSize;   
    SourceMap.GetDimensions(mapSize.x, mapSize.y);

    float depth = ReadDepth(texCoord);    
    float4 color = tex2D(sourceSampler, texCoord);
    float sum = 1.0f;

    for (int i = 0; i < 16; i++)
    {
        float2 tc = texCoord; // TODO: why do I use mapsize here as its in texture coordinates??
        tc.x += (Offsets[i].x * SampleRadius) / mapSize.x;
        tc.y += (Offsets[i].y * SampleRadius) / mapSize.y;

        float blurDepth = ReadDepth(tc);
        float distance = abs(blurDepth - depth) * MaxDistance;
                
        if (distance <= MaxBlurDistance)
        {
            sum += 1.0f;
            color += tex2D(sourceSampler, tc);
        }
    }

    return float4(color / sum);
}

technique AmbientLightTechnique
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}