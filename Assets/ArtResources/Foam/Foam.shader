Shader "Unlit/Foam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="overlay" }
        LOD 100
          
        Pass
        {
            ZWrite Off
            //ZTest Always
            Blend one one

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Water/GerstnerWave.cginc"
            #include "UnityCG.cginc"

            float4x4 GridWorldToLocal[9];

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            v2f vert (appdata v)
            {
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                int index = FindSelfGridIndex(worldPos.xyz);
                float3 localPos = mul(GridWorldToLocal[index], worldPos).xyz;
                
                float3 gerstnerPos = SamplePosition(localPos, _Time.y);
                v.vertex.y = gerstnerPos.y + 0.2f;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * tex2D(_MaskTex, i.uv) * i.color.a;
                return col;
            }
            ENDCG
        }
    }
}
