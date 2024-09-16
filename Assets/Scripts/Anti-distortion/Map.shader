Shader "Custom/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Width("Width", Float) = 1280
        _Height("Height", Float) = 720
        _EyeOffsetX("Eye Offset X", float) = 0.0
        _EyeOffsetY("Eye Offset Y", float) = 0.0
        _ScreenOffsetX("Screen Offset X", float) = 0.0
        _ScreenOffsetY("Screen Offset Y", float) = 0.0
        _K_R("K_R", Vector) = (1, 1, 1)
        _K_G("K_G", Vector) = (1, 1, 1)
        _K_B("K_B", Vector) = (1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Func.cginc"
            // #pragma multi_compile_fog
            // #pragma multi_compile_fwdbase
            // #pragma shader_feature 
            
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Width;
            float _Height;
            float pixelsize;
            float _EyeOffsetX;
            float _EyeOffsetY;
            float _ScreenOffsetX;
            float _ScreenOffsetY;
            float3 _K_R;
            float3 _K_G;
            float3 _K_B;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.x = 1-o.uv.x;
                o.uv.y = 1-o.uv.y;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5 + _EyeOffsetX, 0.5 + _EyeOffsetY);
                float2 uv_r = calculate_uv(i.uv, center, _MainTex_TexelSize.zw * pixelsize, float2(_Width, _Height), _K_R) + float2(_ScreenOffsetX, _ScreenOffsetY);
                float2 uv_g = calculate_uv(i.uv, center, _MainTex_TexelSize.zw * pixelsize, float2(_Width, _Height), _K_G) + float2(_ScreenOffsetX, _ScreenOffsetY);
                float2 uv_b = calculate_uv(i.uv, center, _MainTex_TexelSize.zw * pixelsize, float2(_Width, _Height), _K_B) + float2(_ScreenOffsetX, _ScreenOffsetY);
                float r = tex2D(_MainTex, uv_r).r;
                float g = tex2D(_MainTex, uv_g).g;
                float b = tex2D(_MainTex, uv_b).b;
                return fixed4(r, g, b, 1);
            }
            
            ENDCG
        }
    }
}
