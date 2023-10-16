Shader "Custom RP/Unlit" 
{
	Properties
	{
		_BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		
		// 颜色混合配置. Src 表示当前绘制的内容, Dst 表示之前绘制的内容和最终结果(颜色缓存)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
	}
	
	SubShader 
	{
		Pass
		{
			// [properties] 可访问着色器属性
			Blend [_SrcBlend] [_DstBlend]
			
			HLSLPROGRAM
			#pragma multi_compile_instancing	// 会生成两种变体: 支持 GPU Instance 和不支持 GPU Instance
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
		}
	}
}