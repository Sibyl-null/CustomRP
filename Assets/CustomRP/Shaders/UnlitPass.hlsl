#ifndef CUSTOM_UNLIT_PASS_INCLUDE
#define CUSTOM_UNLIT_PASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"

// 并不是所有平台都支持 cbuffer, 所以使用宏定义(在不支持的平台上相当于没有), 而不是 cbuffer{ xx };
// 自定义属性若要支持 srp batch 需要包裹在 UnityPerMaterial 的 cbuffer 中
CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
CBUFFER_END

float4 UnlitPassVertex(float3 positionOS : POSITION) : SV_POSITION
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    return TransformWorldToHClip(positionWS);
}

float4 UnlitPassFragment() : SV_TARGET
{
    return _BaseColor;
}

#endif