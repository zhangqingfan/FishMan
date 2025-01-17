Shader "Unlit/Water"
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

            #include "GerstnerWave.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 vertex : POSITION;
                float3 normal : TEXCOORD0;
            };


            v2f vert (appdata v)
            {
                Wave wave = SampleWave(v.vertex, _Time.y);
                float4 pos = float4(wave.pos.x, wave.pos.y, wave.pos.z, 1.0);
                
                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.normal = wave.normal;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(1,1,1,1);
                return col;
            }
            ENDCG
        }
    }
}
