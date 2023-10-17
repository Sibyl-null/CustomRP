#ifndef CUSTOM_LIGHTING_INCLUDE
#define CUSTOM_LIGHTING_INCLUDE

#include "Surface.hlsl"
#include "Light.hlsl"

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

// 返回最终照明颜色
float3 GetLighting(Surface surface)
{
    float3 color = 0.0;
    for (int i = 0; i < GetDirectionalLightCount(); ++i)
    {
        color += IncomingLight(surface, GetDirectionalLight(i));
    }
    return color;
}

#endif