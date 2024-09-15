Shader "Custom/PerspectiveRefractionShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _RefractionTex ("Refraction Texture", 2D) = "white" {}
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.5
        _RefractionDistortion ("Refraction Distortion", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _RefractionTex;
            float _RefractionStrength;
            float _RefractionDistortion;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the main texture color
                half4 baseColor = tex2D(_MainTex, i.uv);

                // Sample the refraction texture
                float2 refractionUV = i.uv;

                // Calculate the refraction offset based on the normal and distortion strength
                float2 refractionOffset = (tex2D(_RefractionTex, i.uv).rg - 0.5) * 2.0 * _RefractionDistortion;

                // Apply the refraction offset
                refractionUV += refractionOffset * _RefractionStrength;

                // Sample the refraction texture with the distorted UV
                half4 refractionColor = tex2D(_RefractionTex, refractionUV);

                // Blend the base color with the refraction color
                half4 finalColor = lerp(baseColor, refractionColor, _RefractionStrength);
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
