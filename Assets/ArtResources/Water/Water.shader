Shader "Water"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "Queue"="Transparent"  }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "GerstnerWave.cginc"

            sampler2D _ReflectionTex;
            float4 _ReflectionTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float3 normal : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                Wave wave = SampleWave(v.vertex, _Time.y);

                v2f o;
                o.vertex = UnityObjectToClipPos(wave.pos); 
                o.normal = mul(unity_ObjectToWorld, wave.normal);
                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                //todo...
                float3 worldPos = 0;
                float3 dir = _WorldSpaceCameraPos - worldPos;
                dir = dir / length(dir); 
                
                screenUV += i.normal.zx * half2(0.02, 0.15);

                fixed4 col = tex2D(_ReflectionTex, screenUV);

                //col = fixed4(1,1,1,1);
                return col;
            }
            ENDCG
        }
    }
}
