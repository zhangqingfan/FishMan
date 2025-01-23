Shader "Water"
{
    Properties
    {
        _DepthScale("Depth Scale", Range(1, 2)) = 1
        _AbsorbScale("Absorb Scale", Range(0, 1)) = 0.5
        _DistortScale("Distort Scale", Range(0, 1)) = 0.1

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

            #include "GerstnerWave.cginc"
            #include "UnityLightingCommon.cginc"

            sampler2D _ReflectionTex;
            //float4 _ReflectionTex_ST;

            sampler2D _CameraDepthTexture;
            sampler2D _CameraOpaqueTexture;
            
            float _DepthScale;
            float _AbsorbScale;
            float _DistortScale;
            int _WaterDepth;
            float4 _SurfaceColor;
            float4 _DeepColor;
            
            half GetUnderWaterLength(float2 screenUV, float2 depth_distance)
            {
                float depth = tex2D(_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth);

                //exceed maximum water depth, so it is camera far plane, nothing under water. return 0 means it is the water surface.
                //if(linearDepth > 200)//)
                 //   return 10;    

                float totalLength = linearDepth * depth_distance.y / depth_distance.x;
                float underWaterLength = totalLength - depth_distance.y;

                return underWaterLength;
            }

            half4 GetAbsorbColor(float2 distortUV, half underWaterLength)
            {
                float4 color = tex2D(_CameraOpaqueTexture, distortUV);
                float t = clamp(underWaterLength / _WaterDepth * _DepthScale, 0.0, 1.0);
                float4 waterColor = lerp(_SurfaceColor, _DeepColor, t); 
                //return float4(0, 1, 0, 1);
                return color;//*  waterColor;
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
                o.depth_distance.x = viewPos.z;
                o.depth_distance.y = length(viewPos);
                
                o.worldPos = mul(unity_ObjectToWorld, wave.pos);
                TRANSFER_SHADOW(o);
                return o;
            }

            float4 GetLightColor(float4 albedo, float3 worldNormal, float3 worldPos, float shadowAtten)
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
                float4 col = (0, 0, 0, 1);
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                fixed shadow = SHADOW_ATTENUATION(i);
                //col  = tex2D(_ReflectionTex, screenUV + i.normal.zx * half2(0.02, 0.15));

                //col =  GetLightColor(col, i.normal, i.worldPos, shadow);
                //return col;
                half underWaterLength = GetUnderWaterLength(screenUV, i.depth_distance);
                if(underWaterLength < 0)
                {
                    //return float4(1, 1, 1, 1);

                    float2 distortUV = screenUV;// + i.normal.xz * (i.depth_distance.x * _DistortScale);
                    underWaterLength = GetUnderWaterLength(distortUV, i.depth_distance);
                    col = GetAbsorbColor(distortUV, underWaterLength);
                    return col;
                }
                               
                //underWaterLength < 0 above water
                //underWaterLength > 0 under water 
                //underWaterLength = 0 water surface
                
                col  = tex2D(_ReflectionTex, screenUV);
                return col;
            }
            ENDCG
        }
    }
}
