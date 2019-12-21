// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "GOTerrain/LowPolyMobile3.2 Water And Trasparency" {
	Properties{

	_Color("Color", Color) = (1,1,1,1)

	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Shininess ("Shininess", Range (-10, 10)) = 1.5
	_TexQuant ("LowPolyTextureDetail", Range (1, 256)) = 1

	//WATER
	  _Speed ("Speed", Range (0.01, 200)) = 1.3
	  _Offset ("Offset", Range (-5, 5)) = 0

	  _Ampitude ("Ampitude", Range (0.0, 10)) = 0.0
	  _Alpha ("Alpha", Range(0.1, 1.0)) = 1.0


	}


	Category {
       Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
         ZWrite On
         Blend SrcAlpha OneMinusSrcAlpha

	SubShader {
		Pass {
		    Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
		    #include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"

			uniform float4 _Color;
			uniform float _AmbientLight;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float _Shininess;
			uniform int _TexQuant;
//
//			uniform float _TESTA;
//			uniform float _TESTB;
//

			//WATER
			uniform float _Speed;
			uniform float _Ampitude;
			uniform float _Alpha;
			uniform float _Offset;

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 diff : COLOR1;
				fixed4 color : COLOR;
				float4 worldPos : TEXCOORD1;
				half2 texcoord : TEXCOORD0;
				SHADOW_COORDS(4)
				fixed3 ambient : COLOR2;

			};

			float rand(float3 co){
					return frac(sin(dot(co.xyz ,float3(654.4324,43.321,32.432))) * 25687.5453);
				}

			v2f vert (appdata_full v){

				v2f o;

				//WATER
				if(_Ampitude>0){
					float3 offset = float3(0,0,0);
					offset.y += (sin(_Time*rand(v.vertex.xzz)*_Speed)+1.0)*_Ampitude + _Offset;
					v.vertex.xyz += mul((float3x3)unity_WorldToObject, offset);
				}

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = v.vertex;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);

                o.diff = v.color * _LightColor0 ;

                #ifndef DIRLIGHTMAP_ON
                 o.diff = v.color;
                #endif

 				o.ambient = ShadeSH9(half4(worldNormal,1)) ;

				return o;
			}
			
			fixed4 frag (v2f IN) : SV_Target{

			float3 derivedNormal = cross( normalize(ddx( IN.worldPos.xyz)), normalize(ddy(IN.worldPos.xyz)));
			float magnitudeNormal =  0.0+ ( abs(derivedNormal.x) +abs(derivedNormal.y)+abs(derivedNormal.z) )/3;

			fixed3 lighting =  _Color ;
			lighting *= IN.diff; // Directional Light
			lighting *= IN.ambient * _Shininess; // Ambient Ligh
			lighting *= magnitudeNormal*3;

			//Texture
			float2 texCord =  IN.texcoord;
			texCord.x = ((int)(100*texCord.x/_TexQuant)*_TexQuant) *0.01;
			texCord.y = ((int)(100*texCord.y/_TexQuant)*_TexQuant) *0.01;

			fixed4 col = tex2D(_MainTex, texCord) ;
			col.rgb *= lighting ;
			col.a = _Alpha;
			return col;
			}
			ENDCG 
		}

	}
}
}