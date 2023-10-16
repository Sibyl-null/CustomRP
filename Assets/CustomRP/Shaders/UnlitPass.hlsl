#ifndef CUSTOM_UNLIT_PASS_INCLUDE
#define CUSTOM_UNLIT_PASS_INCLUDE

#include "../ShaderLibrary/Common.hlsl"

// 并不是所有平台都支持 cbuffer, 所以使用宏定义(在不支持的平台上相当于没有), 而不是 cbuffer{ xx };
// 自定义属性若要支持 srp batch 需要包裹在 UnityPerMaterial 的 cbuffer 中
// CBUFFER_START(UnityPerMaterial)
//     float4 _BaseColor;
// CBUFFER_END

// 纹理和其采样器状态是着色器资源。不能按实例提供，必须在全局范围内声明
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);       // 纹理采样器, 控制纹理的采样方式(考虑环绕和过滤模式)

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)    // 纹理平铺和偏移可以针对每个实例进行设置
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);                 // 允许着色器函数访问实例 ID
    UNITY_TRANSFER_INSTANCE_ID(input, output);      // 将实例 ID 从顶点着色器传到片元着色器
    
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMapColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);;
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    return baseMapColor * baseColor;
}

#endif