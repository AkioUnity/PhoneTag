// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GOTerrain/LowPolyMobile3.2" {
	Properties{

	_Color("Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_TexQuant ("TextureDetail for LowPoly", Range (1, 256)) = 1

	_ShininessComponent ("Directional Light", Range (0, 2)) = 0.5
	_AmbientComponent ("Ambient Light", Range (0, 2)) = 0.5
	_Lowpolyness ("Lowpolyness", Range (0.2, 1)) = 0.5

	}


	Category {
   			Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

	SubShader {
		Pass {
		    Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase 

			#include "UnityCG.cginc"
            #include "AutoLight.cginc"

			uniform float4 _Color;
			uniform float _AmbientLight;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float _ShininessComponent;
			uniform float _AmbientComponent;
			uniform float _Lowpolyness;
			uniform int _TexQuant;


			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 diff : COLOR1;
				fixed4 color : COLOR;
				float4 worldPos : TEXCOORD1;
				half2 texcoord : TEXCOORD0;
				fixed3 ambient : COLOR2;
                LIGHTING_COORDS(3,4)

			};


			v2f vert (appdata_full v){

				v2f o;
			
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = v.vertex;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.diff = v.color ;// * _LightColor0 ;

                #ifndef DIRLIGHTMAP_ON
                 o.diff = v.color;
                #endif
   
 				o.ambient = ShadeSH9(half4(worldNormal,1)) ;

			    TRANSFER_VERTEX_TO_FRAGMENT(o);   

				return o;
			}
			
			fixed4 frag (v2f IN) : SV_Target{

			float3 derivedNormal = cross( normalize(ddx( IN.worldPos.xyz)), normalize(ddy(IN.worldPos.xyz)));
			float magnitudeNormal =  0.35+ ( abs(derivedNormal.x) +abs(derivedNormal.y)+abs(derivedNormal.z) )/(3.5*_Lowpolyness);
		//	fixed shadow = SHADOW_ATTENUATION(IN);
			fixed shadow = LIGHT_ATTENUATION(IN);
		
		 	fixed3 lighting = _Color * IN.diff * shadow ;

		 	float lightComponent = ((IN.ambient * _AmbientComponent) + (IN.diff	  * _ShininessComponent))/
		 								2*(_ShininessComponent + _AmbientComponent) ;
			

			//Texture
			float2 texCord =  IN.texcoord;
			texCord.x = ((int)(100*texCord.x/_TexQuant)*_TexQuant) *0.01;
			texCord.y = ((int)(100*texCord.y/_TexQuant)*_TexQuant) *0.01;

			fixed4 col = tex2D(_MainTex, texCord) ;
			col.rgb *= lighting * magnitudeNormal * lightComponent ;


			return col;
			}
			ENDCG 
		}


        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	 }
	}
}
