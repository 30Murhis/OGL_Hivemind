// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/2D World Bend"
{
	Properties
	{
		_Curvature("_Curvature", Float) = 2

		_QOffset("_QOffset", Float) = 2
		_Distance("_Distance", Float) = 2

		_Frequency("Frequency", Range(-0.005, 0.005)) = -0.003
		_Amplitude("Amplitude", Float) = 0.03

		// Earlier version
		//_BendAmount("Bend Factor", Range(-0.1,0.1)) = -0.02
		//_BendDamper("Sine Wave Divider", Float) = 270

		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float _Distance;
			float _QOffset;
			float _Curvature;

			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				//v2f OUT;
				//float4 vv = mul(unity_ObjectToWorld, v.vertex);
				//vv.xyz -= _WorldSpaceCameraPos.xyz;

				////Curvature and taper effects are calculated here
				//vv = float4(((vv.x * vv.y) * _Curvature), (vv.x * vv.x) * -_Curvature, 0.0f, 0.0f );

				////Use this instead if you don't want the taper effect
				////vv = float4( 0.0f, (vv.x * vv.x) * - _Curvature, 0.0f, 0.0f );

				//OUT.vertex = mul(UNITY_MATRIX_MVP, v.vertex) + mul(unity_WorldToObject, vv);
				//OUT.uv = v.uv;
				////OUT.color = v.color * _Color;
				////OUT.vertex = UnityPixelSnap(OUT.vertex);

				//return OUT;



				//v2f o;
				//// The vertex position in view-space (camera space)
				//float4 vPos = mul(UNITY_MATRIX_MV, v.vertex);

				//// Get distance from camera and scale it down with the global _Dist parameter
				//float zOff = vPos.z / _Distance;
				//// Add the offset with a quadratic curve to the vertex position
				//vPos += _QOffset*zOff*zOff;

				//o.vertex = mul(UNITY_MATRIX_P, vPos);
				//o.uv = v.uv;
				//return o;


				//v2f o;
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv.y = pow(v.uv.y * _Distance * UNITY_MATRIX_MV, 2);// +_Distance * v.uv.y;
				//o.uv.x = v.uv.x;
				//return o;

				//v2f o;
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv.y = v.uv.y;
				//o.uv.x = pow(v.uv.x, 2) + _Distance * v.uv.x;;
				//return o;

				// Original
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			float _Frequency;
			float _Amplitude;

			// Earlier version
			//float _BendDamper;
			//float _BendAmount;

			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2D(_MainTex, pow(i.uv,2));

				//float sine = abs(0.5 - i.vertex.x) * 0.001;
				float sine = sin(i.vertex.x * _Frequency) * _Amplitude;
				fixed4 col = tex2D(_MainTex, i.uv + float2(0, sine));

				// Earlier version
				//fixed4 col = tex2D(_MainTex, i.uv + float2(0, sin(i.vertex.x / _BendDamper) * _BendAmount));

				// Original
				//fixed4 col = tex2D(_MainTex, i.uv);

				return col;
			}
			ENDCG
		}
	}
}
