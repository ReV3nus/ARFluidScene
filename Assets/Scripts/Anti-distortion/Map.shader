Shader "Custom/Map"
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 center = float2(0.5 + _EyeOffsetX, 0.5 + _EyeOffsetY);
                float2 xy_r = uv2xy(i.uv, center, _D, _F_R, 1.0 / _MainTex_TexelSize, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));
                float2 xy_g = uv2xy(i.uv, center, _D, _F_G, 1.0 / _MainTex_TexelSize, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));
                float2 xy_b = uv2xy(i.uv, center, _D, _F_B, 1.0 / _MainTex_TexelSize, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));
                float r, g, b;
                if (_Mask > 0) {
                    r = tex2D(_CameraDepthTexture, xy_r);
                    g = tex2D(_CameraDepthTexture, xy_g);
                    b = tex2D(_CameraDepthTexture, xy_b);
                    if (r > 0) {
                        r = 0.0;
                    } else {
                        r = 1.0;
                    }
                    if (g > 0) {
                        g = 0.0;
                    } else {
                        g = 1.0;
                    }
                    if (b > 0) {
                        b = 0.0;
                    } else {
                        b = 1.0;
                    }
                }
                else {
                    r = tex2D(_MainTex, xy_r).r;
                    g = tex2D(_MainTex, xy_g).g;
                    b = tex2D(_MainTex, xy_b).b;
                }
                
                return fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
