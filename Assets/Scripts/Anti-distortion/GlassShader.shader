Shader "Custom/ReflectiveShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "white" {}
        _ReflectStrength ("Reflection Strength", Range(0, 1)) = 1.0
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
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _ReflectionTex;
            float _ReflectStrength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; // Adjust this if needed for proper UV mapping
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 refl = tex2D(_ReflectionTex, i.uv);
                return half4(refl.rgb * _ReflectStrength, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
