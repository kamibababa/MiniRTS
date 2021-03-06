#include "Pack.hlsl"

texture Diffuse;
sampler diffuseSampler = sampler_state
{
    Texture = (Diffuse);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture Normal;
sampler normalSampler = sampler_state
{
    Texture = (Normal);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture Depth;
sampler depthSampler = sampler_state
{
    Texture = (Depth);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture Material;
sampler materialSampler = sampler_state
{
    Texture = (Material);
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct Mat
{
    float Metalicness;
    float Roughness;
    float AmbientOcclusion;
};

float3 ReadDiffuse(float2 texCoord)
{
    return tex2D(diffuseSampler, texCoord).rgb;
}

float3 ReadNormal(float2 texCoord)
{
    float3 normal = tex2D(normalSampler, texCoord).xyz;
    return UnpackNormal(normal);
}

float ReadDepth(float2 texCoord)
{
    return tex2D(depthSampler, texCoord).r;
}

float3 ReadWorldPosition(float2 texCoord, float4x4 inverseViewProjection)
{
    // Read depth
    float depthVal = ReadDepth(texCoord);

    // Compute screen-space position
    float4 position;
    position.x = texCoord.x * 2.0f - 1.0f;
    position.y = -(texCoord.y * 2.0f - 1.0f);
    position.z = depthVal;
    position.w = 1.0f;

    // Transform to world space
    position = mul(position, inverseViewProjection);
    position /= position.w;

    return position.xyz;
}

Mat ReadMaterial(float2 texCoord)
{
    Mat material = (Mat)0;

    float3 mat = tex2D(materialSampler, texCoord).xyz;

    material.Metalicness = mat.x;
    material.Roughness = mat.y;
    material.AmbientOcclusion = mat.z;

    return material;
}
