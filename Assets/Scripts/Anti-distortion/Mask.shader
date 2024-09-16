Shader "Custom/Mask"
{
    Properties
    {
//        _Thick("ThicknessBuffer weights", float) = 0
//        _Depth("DepthBuffer weights", float) = 0
//        _Bright("Brightness", float) = 0
//        _Color("ColorBuffer weights", float) = 0
//        _Alpha("AlphaBuffer weights", float) = 0
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
                float3 viewVec : TEXCOORD01;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D colorBuffer;
            sampler2D depthBuffer;
            sampler2D thicknessBuffer;
            sampler2D _GlobalTransparencyTexture;
            sampler2D _CurveTex;

            UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
            
            float _Thick;
            float _Depth;
            float _Bright;
            float _Color;
            float _Alpha;
            float _Shadow;
            float _Transparency;
            
            float4 _MainTex_ST;
            float2 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // o.uv.x = 1-o.uv.x;
                o.uv.y = 1-o.uv.y;

                float3 ndcPos = float3(o.uv.xy * 2.0 - 1.0, 1);
				float far = _ProjectionParams.z;
				float3 clipVec = float3(ndcPos.x, ndcPos.y, ndcPos.z * -1) * far;
				o.viewVec = mul(unity_CameraInvProjection, clipVec.xyzz).xyz;

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 flippedUVs = i.uv;
                // flippedUVs.y = i.uv.y - _EyeOffsetY/_ScreenSize ;
                // flippedUVs.x = _EyeOffsetX/_ScreenSize - i.uv.x;

                float thick,depth,color,alpha,transparency,shadow;
                float mix = 0;

                if(_Thick>0)
                {
                    thick = tex2D(thicknessBuffer, flippedUVs).r * _Bright;
                    mix += _Thick * thick;
                }
                if(_Depth > 0)
                {
                    depth = 1-Linear01Depth(tex2D(depthBuffer, i.uv)*255);
                    mix += _Depth*depth;
                }
                if(_Color > 0)
                {
                    color = tex2D(_MainTex, flippedUVs).r + tex2D(_MainTex, flippedUVs).b + tex2D(_MainTex, flippedUVs).g;
                    if(color >0) color = 1;

                    mix += _Color*color;
                }
                if(_Alpha > 0)
                {
                    alpha = tex2D(_MainTex, flippedUVs).a;
                    mix += _Alpha*alpha;
                }
                if(_Transparency > 0)
                {
                    transparency = 1-tex2D(_CurveTex, float2(tex2D(_GlobalTransparencyTexture, i.uv).r, 0)).r;
                    mix += _Transparency*transparency;
                }
                if(_Shadow > 0)
                {
                    float cameraDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));
				    float3 worldPos = mul(unity_CameraToWorld, float4(i.viewVec * cameraDepth,1)).xyz;
                    float4 shadowCoord = mul(unity_WorldToShadow[0], float4(worldPos, 1));
                    shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, shadowCoord);
                    if ( cameraDepth > 0.9) shadow = 1;
                    shadow = 1-smoothstep(0.5, 1, shadow);
                    mix += _Shadow*shadow;
                }

                mix = 1-mix;
                return fixed4(mix, mix, mix, 1);
            }
            
            
            ENDCG
        }
    }
}
