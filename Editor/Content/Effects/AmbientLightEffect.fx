// Computes the ambient light in the scene using Screen Space Ambient Occlusion.
// Inspired by: http://ogldev.atspace.co.uk/www/tutorial45/tutorial45.html

#include "Includes/Defines.hlsl"
#include "Includes/Matrices.hlsl"
#include "Includes/GBuffer.hlsl"
#include "Includes/Helpers.hlsl"

static const float Pi = 3.1415926535f;
static const int KERNEL_SIZE = 64;

float NormalOffset = 0.15f;
float3 Color;
float3 Kernel[KERNEL_SIZE];

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

Texture2D FilteredDepthMap : register(t0);
SamplerComparisonState FilteredDepthMapSampler : register(s0);

texture NoiseMap;
sampler noiseSampler = sampler_state
{
    Texture = (NoiseMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
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

    float3 normal = tex2D(normalSampler, texCoord).xyz;
    float3 position = ReadWorldPosition(texCoord, InverseViewProjection).xyz + (normal * NormalOffset);
    float ambientLight = 0.0f;
    for (int i = 0; i < KERNEL_SIZE; i++)
    {
        // Rotate the kernel using the pre-generated noise, seeded by the world position
        // of the object
        int ix = (int)(position.x * 73856093);
        int iy = (int)(position.y * 50000059);
        int iz = (int)(position.z * 83492791);
        float2 uv = (ix ^ iy ^ iz) * 0.0001f;
        
        float3 noise = tex2D(noiseSampler, uv).rgb * (Pi / 2.0f);
        
        float3 offset;
        offset.x = (cos(noise.x) - sin(noise.x)) * Kernel[i].x;
        offset.y = (sin(noise.y) + cos(noise.y)) * Kernel[i].y;
        offset.z = (cos(noise.x) - sin(noise.z)) * Kernel[i].z;        

        // Generate a random position near the original position        
        float4 sampleWorld = float4(position.xyz + offset, 1.0f);

        // Transform to view space
        float4 sampleView = mul(mul(sampleWorld, View), Projection);
                
        // Check if the random point is occluded or not        
        float2 sampleTex = ToTextureCoordinates(sampleView.xy, sampleView.w);        
        float depth = sampleView.z / sampleView.w;
        
        ambientLight += FilteredDepthMap.SampleCmpLevelZero(FilteredDepthMapSampler, sampleTex, depth);
    }

    ambientLight /= KERNEL_SIZE;
    return float4(Color.rgb * ambientLight, 0.0f);
}

technique AmbientLightTechnique
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}