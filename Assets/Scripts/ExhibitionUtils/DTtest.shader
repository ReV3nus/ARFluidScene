// Shader "ReV3nus/DTtest"
// {
//     Properties
//     {
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "UnityCG.cginc"

//             struct v2f
//             {
//                 float4 pos : SV_POSITION;
// 				float3 worldPos : TEXCOORD0;
//             };

//             v2f vert(appdata_full v)
//             {
//                 v2f o;
//                 o.pos = UnityObjectToClipPos(v.vertex);
// 				o.worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
//                 return o;
//             }
            
//             half4 frag(v2f i) : SV_Target
//             {
//                 float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                
//                 half4 color = tex2D(_MainTex, i.uv);
//                 float transparency = color.a;

//                 return half4(depth, depth, depth, 1.0);
//             }
//             ENDCG
//         }
//     }
//     Fallback Off
// }
