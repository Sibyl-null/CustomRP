#ifndef CUSTOM_SURFACE_INCLUDE
#define CUSTOM_SURFACE_INCLUDE

// 表面效果相关数据结构
// normal 未指定具体空间，根据实际需要赋值
struct Surface
{
    float3 normal;
    float3 color;
    float alpha;
    float metallic;
    float smoothness;
};

#endif