Shader "Custom/Map"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FovX("Fov X", float) = 100.0
        _FovY("Fov Y", float) = 100.0
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
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;
            float _FovX;
            float _FovY;
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
                float2 flippedUVs = i.uv;
                flippedUVs.y = 1 - i.uv.y;
                flippedUVs.x = 1 - i.uv.x;
                
                // sample the texture
                float2 center = float2(0.5 + _EyeOffsetX, 0.5 + _EyeOffsetY);
                float2 xy_r = uv2xy(flippedUVs, center, _FovX, _FovY, _F_R, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));
                float2 xy_g = uv2xy(flippedUVs, center, _FovX, _FovY, _F_G, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));
                float2 xy_b = uv2xy(flippedUVs, center, _FovX, _FovY, _F_B, _ScreenSize, float2(_ScreenOffsetX, _ScreenOffsetY));

                if (xy_r.x < 0 || xy_r.x > 1 || xy_r.y < 0 || xy_r.y > 1) return fixed4(0, 0, 0, 1);
                if (xy_g.x < 0 || xy_g.x > 1 || xy_g.y < 0 || xy_g.y > 1) return fixed4(0, 0, 0, 1);
                if (xy_b.x < 0 || xy_b.x > 1 || xy_b.y < 0 || xy_b.y > 1) return fixed4(0, 0, 0, 1);

                float r = tex2D(_MainTex, xy_r).r;
                float g = tex2D(_MainTex, xy_g).g;
                float b = tex2D(_MainTex, xy_b).b;

                return fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
