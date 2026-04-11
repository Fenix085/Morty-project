Shader "Custom/TextureInsideSurface" {
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		//ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		Cull Front

		CGPROGRAM

			#pragma surface surf Lambert vertex:vert alphatest:_CutOff alpha

			sampler2D _MainTex;
			fixed4 _Color;

			struct Input
			{
				float2 uv_MainTex;
				float4 color : COLOR;
			};

			void vert(inout appdata_full v)
			{
				v.normal.xyz = v.normal * 1;
			}

			void surf (Input IN, inout SurfaceOutput o)
			{
				fixed2 uv = IN.uv_MainTex;
				fixed4 result = tex2D(_MainTex, IN.uv_MainTex);

				o.Albedo = result.rgb;
				o.Alpha = result.a;
			}

		ENDCG
	}

	Fallback "Diffuse"
}