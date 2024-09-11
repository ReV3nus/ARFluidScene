Shader "ReV3nus/OpaqueTransparency"
{
    Properties
    {
    }
    SubShader
    {
		Name "TransparencyPass"
		Tags { "LightMode" = "AlphaOnly" "RenderType" = "Opaque"}
		ZWrite Off
		ColorMask RGB 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                return half4(0.0, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
