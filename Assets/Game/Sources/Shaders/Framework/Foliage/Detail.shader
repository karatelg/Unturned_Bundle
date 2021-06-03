﻿Shader "Framework/Detail" 
{
	Properties 
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
		}

		LOD 200
		
		CGPROGRAM
			 
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular addshadow
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandardSpecular OUT)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			OUT.Albedo = c.rgb;
			OUT.Alpha = c.a;
			OUT.Specular = 0.0;
		}

		ENDCG
	}

	FallBack "Standard"
}
