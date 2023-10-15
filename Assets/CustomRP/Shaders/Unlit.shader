Shader "Custom RP/Unlit" 
{
	Properties
	{
		_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	
	SubShader 
	{
		Pass
		{
			HLSLPROGRAM
			#pragma multi_compile_instancing	// 会生成两种变体: 支持 GPU Instance 和不支持 GPU Instance
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
		}
	}
}