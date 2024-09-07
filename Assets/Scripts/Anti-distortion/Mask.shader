Shader "Custom/Mask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            sampler2D _CameraDepthTexture;
            sampler2D colorBuffer;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float r, g, b;
                // r = tex2D(_CameraDepthTexture, flippedUVs);
                // g = tex2D(_CameraDepthTexture, flippedUVs);
                // b = tex2D(_CameraDepthTexture, flippedUVs);
                r = tex2D(colorBuffer, i.uv);
                g = tex2D(colorBuffer, i.uv);
                b = tex2D(colorBuffer, i.uv);
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
                
                return fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
