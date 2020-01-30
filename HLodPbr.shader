Shader "Custom/HLodPbr" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		            Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		 #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
				float4 uv1 : TEXCOORD1;
				    fixed4 color : COLOR;

			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float3 diff : TEXCOORD2;
 				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
 			float4 _MainTex_ST;
	 float4 _Color;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 =  v.uv1;
				  
				 half3 worldNormal =  UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal,normalize( _WorldSpaceLightPos0.xyz)));
                o.diff = nl  * _LightColor0*v.color.rgb;

                // the only difference from previous shader:
                // in addition to the diffuse lighting from the main light,
                // add illumination from ambient or light probes
                // ShadeSH9 function from UnityCG.cginc evaluates it,
                // using world space normal
              o.diff.rgb += ShadeSH9(half4(worldNormal,1))/2;
              //  o.diff.rgb += ShadeSH9(half4(worldNormal,1));
 				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
			 //return abs(frac(-1.8)-(0.8))<0.01?1:0;
				float2 uv=i.uv;
				//uv-=(int)(uv);
				uv.x*=i.uv1.z;
				uv.y*=i.uv1.w;
				// if(uv.x<0)uv.x+=1;
				// if(uv.y<0)uv.y+=1;
				uv+=100;
						uv.x-=i.uv1.z*(int)(uv.x/i.uv1.z);
				uv.y-=i.uv1.w*(int)(uv.y/i.uv1.w);
				uv+=i.uv1.xy;
				
		
				fixed4 col = tex2D(_MainTex, uv);
 col.rgb*=i.diff;
				return col;
			}
			ENDCG
		}
		
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0

			// -------------------------------------


			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _METALLICGLOSSMAP
			#pragma shader_feature _PARALLAXMAP
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}
	}
}