Shader "Hidden/DepthAndTransparencyCapture"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
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

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            
            float SelfLinear01Depth(float z)
            {
                float near = 0.3;
                float far = 2;
                return (2.0 * near) / (far + near - z * (far - near));
            }
            half4 frag(v2f i) : SV_Target
            {
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                
                half4 color = tex2D(_MainTex, i.uv);
                float transparency = color.a;

                return half4(depth, depth, depth, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
