#include "Includes/Defines.hlsl"
#include "Includes/Matrices.hlsl"
#include "Includes/GBuffer.hlsl"
#include "Includes/Helpers.hlsl"

float4x4 ProjectorViewProjection;
float4 Tint;
float3 ProjectorPosition;
float3 ProjectorForward;
float MaxDistance;

Texture2D ProjectorMap;
sampler projectorSampler = sampler_state
{
    Texture = (ProjectorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};

Texture2D Mask;
sampler maskSampler= sampler_state
{
    Texture = (Mask);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;    
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;    
    float4 ScreenPosition : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);    
    output.ScreenPosition = output.Position;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float2 texCoord = ToTextureCoordinates(input.ScreenPosition.xy, input.ScreenPosition.w);
    float4 position = ReadWorldPosition(texCoord, InverseViewProjection);

    // Move from world position to the reference frame of the projector
    float4 positionInProjectorReferenceFrame = mul(position, ProjectorViewProjection);

    // Figure out where on the projector map the current pixel is
    float2 projectorMapCoordinates = ToTextureCoordinates(positionInProjectorReferenceFrame.xy, positionInProjectorReferenceFrame.w);
        
    // Distance between pixel and projector
    float dist = distance(ProjectorPosition, position.xyz);

    // Angle between pixel and projector, dir > 0 means the pixel is in front of the projector
    // while dir < 0 means its behind it.
    float3 direction = normalize(position.xyz - ProjectorPosition);
    float dir = dot(ProjectorForward, direction);

    // TODO: also read the normal map and check that direction?

    // Only apply the projector if the it is inside the bounds of the projector texture, close enough, and in front of the projector
    if (dir > 0 && dist < MaxDistance &&
        projectorMapCoordinates.x >= 0.0f && projectorMapCoordinates.x <= 1.0f &&
        projectorMapCoordinates.y >= 0.0f && projectorMapCoordinates.y <= 1.0f)
    {        		       
        float mask = tex2D(maskSampler, projectorMapCoordinates).r;
        return tex2D(projectorSampler, projectorMapCoordinates) * Tint * mask;
    }
    
    return float4(0, 0, 0, 0);
}

// Copied MainPS that displays texture coordinates when out of projector space
float4 OverdrawPS(VertexShaderOutput input) : COLOR0
{
    float2 texCoord = ToTextureCoordinates(input.ScreenPosition.xy, input.ScreenPosition.w);
    float4 position = ReadWorldPosition(texCoord, InverseViewProjection);

    // Move from world position to the reference frame of the projector
    float4 positionInProjectorReferenceFrame = mul(position, ProjectorViewProjection);

    // Figure out where on the projector map the current pixel is
    float2 projectorMapCoordinates = ToTextureCoordinates(positionInProjectorReferenceFrame.xy, positionInProjectorReferenceFrame.w);

    // Distance between pixel and projector
    float dist = distance(ProjectorPosition, position.xyz);

    // Angle between pixel and projector, dir > 0 means the pixel is in front of the projector
    // while dir < 0 means its behind it.
    float3 direction = normalize(position.xyz - ProjectorPosition);
    float dir = dot(ProjectorForward, direction);

    // TODO: also read the normal map and check that direction?

    // Only apply the projector if the it is inside the bounds of the projector texture, close enough, and in front of the projector
    if (dir > 0 && dist < MaxDistance &&
        projectorMapCoordinates.x >= 0.0f && projectorMapCoordinates.x <= 1.0f &&
        projectorMapCoordinates.y >= 0.0f && projectorMapCoordinates.y <= 1.0f)
    {
        float mask = tex2D(maskSampler, projectorMapCoordinates).r;
        return tex2D(projectorSampler, projectorMapCoordinates) * Tint * float4(1.0f, 1.0f, 1.0f, mask);
    }

    return float4(texCoord.x, texCoord.y, 0.0f, 0.0f);    
}

technique ProjectorEffect
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}

technique ProjectorOverdrawEffect
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL OverdrawPS();
    }
}