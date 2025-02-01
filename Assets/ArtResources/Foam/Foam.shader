Shader "Unlit/Foam"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100
          
        Pass
        {
            ZWrite Off
            Blend one one

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Water/GerstnerWave.cginc"
            #include "UnityCG.cginc"

            float4x4 _curGridWorldToLocal;

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
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4x4 _WorldToLocal;

            v2f vert (appdata v)
            {
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float3 curGridLocalPos = mul(_curGridWorldToLocal, worldPos);
                //Wave wave = SampleWave(curGridLocalPos, _Time.y);
                Wave wave = SampleWave(v.vertex, _Time.y);

                v2f o;
                o.vertex = UnityObjectToClipPos(wave.pos);
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
