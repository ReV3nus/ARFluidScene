Shader "Custom/Mask"
{
    Properties
    {
        _Mask("Show Mask", float) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _D("D", float) = 1.0
        _ScreenSize("Screen Size", float) = 1.0
        _EyeOffsetX("Eye Offset X", float) = 0.0
        _EyeOffsetY("Eye Offset Y", float) = 0.0
        _ScreenOffsetX("Screen Offset X", float) = 0.0
        _ScreenOffsetY("Screen Offset Y", float) = 0.0
        _F_R("F_R", float) = 1.0
        _F_G("F_G", float) = 1.0
        _F_B("F_B", float) = 1.0
        _F_A("F_A", float) = 1.0

        
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
            sampler2D _CameraDepthTexture;
            sampler2D colorBuffer;
            sampler2D depthBuffer;

            float _Mask;
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;
            float _D;
            float _ScreenSize;
            float _EyeOffsetX;
            float _EyeOffsetY;
            float _ScreenOffsetX;
            float _ScreenOffsetY;
            float _F_R;
            float _F_G;
            float _F_B;
            float _F_A;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // fixed4 frag (v2f i) : SV_Target
            // {
            //     float2 flippedUVs = i.uv;
            //     flippedUVs.y = i.uv.y - _EyeOffsetY/_ScreenSize ;
            //     flippedUVs.x = _EyeOffsetX/_ScreenSize - i.uv.x;
            //     float linearDepth;
            //     linearDepth = tex2D(depthBuffer, flippedUVs);
            //     linearDepth = Linear01Depth(linearDepth*255) + 0.5f;
            //     
            //     return fixed4(linearDepth,linearDepth,linearDepth,linearDepth);
            //
            // }

            fixed4 frag (v2f i) : SV_Target
            {
                float c = 1.0;
                c = tex2D(_MainTex, i.uv).r;
                c = 1-c;
                return fixed4(c, c, c, 1);
            }
            ENDCG
        }
    }
}
