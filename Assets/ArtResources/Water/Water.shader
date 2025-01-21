Shader "Water"
{
    Properties
    {
        _AbsorbScale("Absorb Scale", Range(0, 1)) = 0.5
        _DepthScale("Depth Scale", Range(10, 30)) = 20
        _DistortScale("Distort Scale", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent"  }
        LOD 100

        Pass
        {
            ZWrite Off
            //Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "GerstnerWave.cginc"

            sampler2D _ReflectionTex;
            //float4 _ReflectionTex_ST;

            sampler2D _CameraDepthTexture;
            sampler2D _CameraOpaqueTexture;

            float _AbsorbScale;
            float _DepthScale;
            float _DistortScale;

            half GetUnderWaterLength(float2 screenUV, float2 depth_distance)
            {
                float depth = tex2D(_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth);
                
                float totalLength = linearDepth * depth_distance.y / depth_distance.x;
                float underWaterLength = totalLength - depth_distance.y;

                return underWaterLength;
            }

            half4 GetAbsorbColor(float2 distortUV, half underWaterLength) 
            {
                float4 color = tex2D(_CameraOpaqueTexture, distortUV);
                float absorbFactor = exp(-_AbsorbScale * underWaterLength);
                absorbFactor = saturate(absorbFactor);

                return color * absorbFactor;
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
            };

            v2f vert (appdata v)
            {
                Wave wave = SampleWave(v.vertex, _Time.y);

                v2f o;
                o.vertex = UnityObjectToClipPos(wave.pos); 
                o.normal = mul(unity_ObjectToWorld, wave.normal);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                float4 viewPos = mul(UNITY_MATRIX_MV, wave.pos);
                o.depth_distance.x = viewPos.z;
                o.depth_distance.y = length(viewPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = (1, 1, 1, 1);
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                                
                //underWaterLength < 0 above water
                //underWaterLength > 0 under water
                //underWaterLength > 1000 nothing exist under water, just camera far plane
                half  underWaterLength = GetUnderWaterLength(screenUV, i.depth_distance);
                                
                //float2 uvOffset = (underWaterLength < 0 || underWaterLength > 1000) ? i.normal.zx * half2(0.02, 0.15) : i.normal.zx * saturate(i.depth_distance.x * _DistortScale); 
                //screenUV += uvOffset;

                col = (underWaterLength < 0 || underWaterLength > 1000)? tex2D(_ReflectionTex, screenUV) : GetAbsorbColor(screenUV, underWaterLength);
                
                col.w *= 0.8f;  
                return col;
            }
            ENDCG
        }
    }
}
