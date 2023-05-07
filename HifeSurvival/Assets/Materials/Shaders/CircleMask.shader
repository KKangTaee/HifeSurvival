Shader "Unlit/CircleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _MainTex_ST;

            float4 _Color;
            float _Radius;

            v2f vert (appdata v) 
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target 
            {
                if(_Radius <= 0.01f)
                {
                    return tex2D(_MainTex, i.uv);
                }
                else
                {
                    // 텍스쳐의 가로세로 비율을 보정합니다.
                    // float aspectRatio = _MainTex_ST.y / _MainTex_ST.x;
                     float aspectRatio = _ScreenParams.y / _ScreenParams.x;
                    float2 correctedUV = float2(i.uv.x, i.uv.y * aspectRatio);

                    // 원의 중심은 (0.5, 0.5)입니다.
                    float2 center = float2(0.5, 0.5 * aspectRatio);
                    float dist = distance(correctedUV, center);

                    // 원 안쪽에서부터 점점 반투명해지게 합니다.
                    float alphaFactor = saturate((dist - _Radius) * 10.0);

                    // 텍스처와 색상을 적용하고, 원 안쪽에서부터 반투명해지게 만듭니다.
                    fixed4 col = tex2D(_MainTex, i.uv);
                    col.rgb *= _Color.rgb;
                    col.a *= alphaFactor;
                    return col;
                }
            }
            ENDCG
        }
    }
}
