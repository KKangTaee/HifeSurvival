Shader "Custom/RefractionShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Refraction("Refraction", Range(0.01, 0.1)) = 0.05
        _NormalMap("Normal Map", 2D) = "bump" {}
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            float _Refraction;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 normal = UnpackNormal(tex2D(_NormalMap, i.uv));
                float2 refraction = normal.xy * _Refraction;

                fixed4 col = tex2D(_MainTex, i.uv + refraction);
                return col;
            }
            ENDCG
        }
    }
}