Shader "Custom RP/Unlit" 
{
	Properties
	{
		_BaseMap ("Texture", 2D) = "white" {}
		_BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
		
		// 颜色混合配置. Src 表示当前绘制的内容, Dst 表示之前绘制的内容和最终结果(颜色缓存)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
	}
	
	SubShader 
	{
		Pass
		{
			// [properties] 可访问着色器属性
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING	// 根据是否应用透明度测试，生成变体
			#pragma multi_compile_instancing	// 会生成两种变体: 支持 GPU Instance 和不支持 GPU Instance
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
		}
	}
}