Shader "Water"
{
    Properties
    {
        _DepthScale("Depth Scale", Range(0, 2)) = 1
        _DistortScale("Distort Scale", Range(0, 1)) = 1
        
        _CausticsScale("Caustics Scale", Range(0, 0.3)) = 0.01
        _CausticsnItensity("Caustics Itensity", Range(0, 3)) = 1
        _CausticsTex ("Caustics Texture", 2D) = "white" {}

        _FresnelBias("Fresnel Bias", Range(0, 1)) = 0.02
        _NormalBias("Normal Bias", Range(0, 1)) = 0.02

        _SurfaceColor ("_SurfaceColor", Color) = (1, 1, 1, 1) 
        _DeepColor ("_DeepColor", Color) = (1, 1, 1, 1)
        _NormalTex ("Normal Texture", 2D) = "white" {}

        _FoamMask ("FoamMask", 2D) = "white" {}
        _FoamItensity("FoamItensity", Range(1, 1000)) = 1

        _ContactFoamMask ("ContactFoamMask", 2D) = "white" {}
        _Contactnsity("Contactnsity", Range(0, 10)) = 0.25
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
            #include "WaterRT.cginc"
            #include "WaterFoam.cginc"
            #include "UnityLightingCommon.cginc"

            sampler2D _ReflectionTex;
            float4 _ReflectionTex_ST;
            sampler2D _CausticsTex;
            float4 _CausticsTex_ST;
            sampler2D _NormalTex;
            float4 _NormalTex_ST;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            //sampler2D _CameraDepthTexture;
            sampler2D _CameraOpaqueTexture;
            
            float _CausticsScale;
            float _CausticsnItensity;
            float _DepthScale;
            float _DistortScale;
            int _WaterDepth;
            float4 _SurfaceColor;
            float4 _DeepColor;
            float4x4 _InverseVP;
            float _FresnelBias;
            float _NormalBias;
            float _FoamItensity;
            float _Contactnsity;

            float3 GetUnderWaterWorldPos(float2 screenUV, float3 waterWorldPos, float2 depth_distance)
            {
                float depth = tex2D(_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth);
                float totalLength = linearDepth * depth_distance.y / (depth_distance.x);
                return _WorldSpaceCameraPos + normalize(waterWorldPos - _WorldSpaceCameraPos) * totalLength;
            }

            float GetUnderWaterLength(float2 screenUV, float2 depth_distance)
            {
                float depth = tex2D(_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth);
                float totalLength = linearDepth * depth_distance.y / (depth_distance.x);
                return totalLength - depth_distance.y;
            }

            half4 SampleUnderWaterColor(float2 uv, half underWaterLength)
            {
                float4 color = tex2D(_CameraOpaqueTexture, uv);
                float t = clamp(_DepthScale * underWaterLength / _WaterDepth , 0, 1);
                float4 waterColor = lerp(_SurfaceColor, _DeepColor, t); 
                color *= waterColor;
                return color;
            }

            float4 ApplyCaustics(float2 uv, float3 underWaterWorldPos)
            {
                 //Unity approach to gain world position;
                 //float depth = tex2D(_CameraDepthTexture, uv);
                 //#if defined(unity_reversed_z)
                 //    depth = depth;
                 //#else
                 //    depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
                 //#endif
   
                 //float3 opaqueWorldPos = ComputeWorldSpacePosition_1(uv, depth, _InverseVP);

                //my approach
                 float3 opaqueWorldPos = underWaterWorldPos;
                
                //camera far plane
                if(length(_WorldSpaceCameraPos - opaqueWorldPos) > 500)
                    return float4(0, 0, 0, 0);

                float2 causticsUV = opaqueWorldPos.xz * _CausticsScale;
                causticsUV += 0.1 * sin(_Time.y);

                float4 col = tex2D(_CausticsTex, causticsUV); 
                col *= _CausticsnItensity;
                return col;
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

            float FresnelTerm(float3 worldNormal, float3 worldPos)
            {
                worldNormal = normalize(worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                float fresnel = pow(1.0 - max(dot(viewDir, worldNormal), 0.0), 64.0);
                return _FresnelBias + (1.0 - _FresnelBias) * fresnel;
            }

            float3 DistortNormal(float3 worldPos, float3 worldNormal)
            {
                float2 uv = worldPos.xz + _Time.y * _NormalBias;
                float3 unpackedNormal = normalize(UnpackNormal(tex2D(_NormalTex, uv)));
                return normalize(worldNormal + unpackedNormal);
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float3 normal : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 depth_distance: TEXCOORD2;
                float3 worldPos: TEXCOORD3;
                float4 localPos: TEXCOORD4; //w:grid index
                float originalY: TEXCOORD5; //not used.
                SHADOW_COORDS(6)
            };

            v2f vert (appdata v)
            {   
                Wave wave = SampleWave(v.vertex, _Time.y);
                float4 worldPos = mul(unity_ObjectToWorld, float4(wave.pos.xyz, 1));
                int gridIndex = FindSelfGridIndex(worldPos.xyz);
                float offsetY = SampleTrackRT(gridIndex, wave.pos);
                wave.pos.y -= offsetY;
                
                v2f o;
                o.vertex = UnityObjectToClipPos(wave.pos);
                o.normal = UnityObjectToWorldNormal(wave.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                float3 viewPos = UnityObjectToViewPos(wave.pos);
                o.depth_distance.x = abs(viewPos.z);   // 摄像机空间, 视线所指的方向Z坐标是负数!!!!!
                o.depth_distance.y = length(viewPos);
                
                o.worldPos = mul(unity_ObjectToWorld, float4(wave.pos.xyz, 1)).xyz;
                o.localPos.xyz = wave.pos.xyz;
                o.localPos.w = gridIndex;
                
                o.originalY = wave.pos.y + offsetY;

                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {   
                i.normal += CalculateTrackRTNormal(i.localPos.w, i.localPos.xyz);
                i.normal = normalize(i.normal);
                
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                fixed shadow = SHADOW_ATTENUATION(i);
                
                //bug!!!
                //float2 distortUV = mul(UNITY_MATRIX_VP, float4(i.normal.xyz, 1)).xz;
                float2 distortUV = sin(_Time.y) * i.normal.zx * _DistortScale;
                distortUV += screenUV;

                //underWaterLength < 0 above water
                //underWaterLength > 0 under water 
                float underWaterLength = GetUnderWaterLength(distortUV, i.depth_distance);
                
                //如果扭曲uv后采样到水面之上的物体了，这是不被允许的，所以回退重新采样
                if(underWaterLength < 0)
                {
                    distortUV = screenUV;
                    underWaterLength = GetUnderWaterLength(distortUV, i.depth_distance);
                }

                //if(underWaterLength < 0)
                //    return float4(1, 0, 0, 1);

                float totalLength = underWaterLength + i.depth_distance.y;
                float3 underWaterWorldPos = _WorldSpaceCameraPos + normalize(i.worldPos - _WorldSpaceCameraPos) * totalLength;
                
                float4 underWaterColor = SampleUnderWaterColor(distortUV, underWaterLength);
                float4 causticsColor = ApplyCaustics(distortUV, underWaterWorldPos);
                underWaterColor += causticsColor;

                float4 reflectionColor  = tex2D(_ReflectionTex, screenUV + i.normal.xz * half2(0.02, 0.15));
                reflectionColor = LambertLight(reflectionColor, i.normal, i.worldPos, shadow) * 0.9;
                //reflectionColor = LambertLight(reflectionColor, DistortNormal(i.worldPos, i.normal), i.worldPos, shadow);
                
                float4 finalColor = lerp(underWaterColor, reflectionColor, FresnelTerm(i.normal, i.worldPos));

                float coverage = WaveFoamCoverage(i.localPos.y, normalize(i.normal)) * _FoamItensity +
                                 ContactFoam(i.worldPos.xz, underWaterLength) * _Contactnsity;

                finalColor += float4(GetFoamAlbedo(i.worldPos.xz, coverage).xyz, 1);
                return finalColor;
            }

            ENDCG
        }
    }
}
