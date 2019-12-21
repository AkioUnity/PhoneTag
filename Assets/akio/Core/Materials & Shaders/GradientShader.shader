Shader "Mobile/GradientShader" {
	Properties {
		_Shiness ("Shiness", Range (0.01, 10)) = 1
		_Gloss ("Gloss", Range (0.01, 10)) = 1
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,0)
      _MainTex ("Texture", 2D) = "white" {}

	}
	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong


        float _Shiness;
        float _Gloss;
        sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			half4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo =  IN.color.rgb * tex2D (_MainTex, IN.uv_MainTex).rgb;
			o.Alpha =   IN.color.a;
	        o.Specular = _Shiness;
            o.Gloss = _Gloss;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}