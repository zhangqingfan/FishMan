Shader "Water"
{
    Properties
    {
        _DepthScale("Depth Scale", Range(0, 2)) = 1
        _DistortScale("Distort Scale", Range(0, 5)) = 1
        
        _CausticsScale("Caustics Scale", Range(0, 1)) = 0.01
        _CausticsnItensity("Caustics Itensity", Range(0, 3)) = 1
        _CausticsTex ("Texture", 2D) = "white" {}

        _SurfaceColor ("_SurfaceColor", Color) = (1, 1, 1, 1) 
        _DeepColor ("_DeepColor", Color) = (1, 1, 1, 1) 
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"

            #include "GerstnerWave.cginc"
            #include "UnityLightingCommon.cginc"

            sampler2D _ReflectionTex;
            float4 _ReflectionTex_ST;
            sampler2D _CausticsTex;
            float4 _CausticsTex_ST;

            sampler2D _CameraDepthTexture;
            sampler2D _CameraOpaqueTexture;
            
            float _CausticsScale;
            float _CausticsnItensity;
            float _DepthScale;
            float _DistortScale;
            int _WaterDepth;
            float4 _SurfaceColor;
            float4 _DeepColor;
            float4x4 _InverseVP;
            
            float3 GetUnderWaterWorldPos(float2 screenUV, float3 waterWorldPos, float2 depth_distance)
            {
                float depth = tex2D(_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth);
                float totalLength = linearDepth * depth_distance.y / (depth_distance.x);
                return _WorldSpaceCameraPos + normalize(waterWorldPos - _WorldSpaceCameraPos) * totalLength;
            }

            float GetUnderWaterLength(float3 underWaterWorldPos, float3 waterWorldPos)
            {
                float totalLength = length(underWaterWorldPos - _WorldSpaceCameraPos);
                float aboveWaterLength = length(waterWorldPos - _WorldSpaceCameraPos);
                return totalLength - aboveWaterLength;
            }

            half4 SampleUnderWaterColor(float2 uv, half underWaterLength)
            {
                float4 color = tex2D(_CameraOpaqueTexture, uv);
                float t = clamp(_DepthScale * underWaterLength / _WaterDepth , 0, 1);
                float4 waterColor = lerp(_SurfaceColor, _DeepColor, t); 
                color *= waterColor;
                return color;
            }

            float4 ApplyCaustics(float3 underWaterWorldPos, float underWaterLength)
            {
                // Unity approach to gain world position;
                // float depth = tex2d(_cameradepthtexture, uv);
                // #if defined(unity_reversed_z)
                //     depth = depth;
                // #else
                //     depth = lerp(unity_near_clip_value, 1, depth);
                // #endif
   
                // float3 opaqueworldpos = computeworldspaceposition_1(uv, depth, _inversevp);

                //my approach
                float3 opaqueWorldPos = underWaterWorldPos;
                
                //camera far plane
                if(length(_WorldSpaceCameraPos - opaqueWorldPos) > 500)
                    return float4(0, 0, 0, 0);

                float t = clamp(_DepthScale * underWaterLength / _WaterDepth, 0, 1);
                fixed scale = lerp(0.2, 0.8, 1 - t);

                float2 causticsUV = opaqueWorldPos.xz  * scale * _CausticsScale;
                causticsUV += 0.01 * sin(_Time.y);

                float4 col = tex2D(_CausticsTex, causticsUV); 
                col *= _CausticsnItensity;
                return col;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float3 normal : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 depth_distance: TEXCOORD2;
                float3 worldPos: TEXCOORD3;
                SHADOW_COORDS(4) 
            };

            v2f vert (appdata v)
            {
                Wave wave = SampleWave(v.vertex, _Time.y);

                v2f o;
                o.vertex = UnityObjectToClipPos(wave.pos);
                o.normal = UnityObjectToWorldNormal(wave.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                float4 viewPos = mul(UNITY_MATRIX_MV, wave.pos);
                o.depth_distance.x = abs(viewPos.z);   // 摄像机空间, 视线所指的方向Z坐标是负数!!!!!
                o.depth_distance.y = length(viewPos);
                
                o.worldPos = mul(unity_ObjectToWorld, wave.pos);
                TRANSFER_SHADOW(o);
                return o;
            }

            float4 LambertLight(float4 albedo, float3 worldNormal, float3 worldPos, float shadowAtten)
            {
                float3 diffuse = max(0, dot(worldNormal, _WorldSpaceLightPos0));
                float3 diffuseColor = _LightColor0.rgb * albedo * diffuse;

                float3 viewDir = _WorldSpaceCameraPos.xyz - worldPos;
                float3 halfDir = normalize(_WorldSpaceLightPos0 + viewDir);
                float3 specularColor = float3(1, 1, 1) * pow(max(0, dot(worldNormal, halfDir)), 8);
                
                return float4((diffuseColor + specularColor) * shadowAtten, 1);
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                fixed shadow = SHADOW_ATTENUATION(i);

                //ComputeWorldSpacePosition((screenUV, 1, UNITY_MATRIX_MV);
                // //先不用Distort, 把图像调试正确了
                // float depth = tex2D(_CameraDepthTexture, screenUV);
                // float linearDepth = LinearEyeDepth(depth);
                // float linear01 = Linear01Depth(depth);
                // //return float4(linear01, linear01, linear01, 1);
                
                float2 distortUV = screenUV + sin(_Time.y) * i.normal.zx * _DistortScale;
                float3 underWaterWorldPos = GetUnderWaterWorldPos(distortUV, i.worldPos, i.depth_distance);
                
                //underWaterLength < 0 above water
                //underWaterLength > 0 under water 
                float underWaterLength = GetUnderWaterLength(underWaterWorldPos, i.worldPos);
                
                //如果扭曲uv后采样到水面之上的物体了，这是不被允许的，所以回退重新采样
                if(underWaterLength < 0)
                {
                    distortUV = screenUV;
                    underWaterWorldPos = GetUnderWaterWorldPos(distortUV, i.worldPos, i.depth_distance);
                    underWaterLength = GetUnderWaterLength(underWaterWorldPos, i.worldPos);
                }

                float4 underWaterColor = SampleUnderWaterColor(distortUV, underWaterLength);
                float4 causticsColor = ApplyCaustics(underWaterWorldPos, underWaterLength);
                underWaterColor += causticsColor;

                float4 reflectionColor  = tex2D(_ReflectionTex, screenUV + i.normal.zx * half2(0.02, 0.15));
                //reflectionColor = LambertLight(reflectionColor, i.normal, i.worldPos, shadow);

                return underWaterColor;
            }

            ENDCG
        }
    }
}
