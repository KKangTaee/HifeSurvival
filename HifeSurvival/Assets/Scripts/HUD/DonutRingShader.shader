Shader "Custom/DonutRingShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _RingNum("Number of Rings", Range(1,100)) = 3
        _RingSpeed("Ring Speed", Range(0.1,2.0)) = 0.5
        _Fade("Fade", Range(0.1,1)) = 0.5
    }
        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
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

            fixed4 _Color;
            float _RingNum;
            float _RingSpeed;
            float _Fade;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - float2(0.5,0.5);
                float dist = length(uv);
                float alpha = (sin(_Time.y * _RingSpeed - dist * _RingNum) + 1) * 0.5;
                alpha = 1.0 - smoothstep(_Fade, 1.0, alpha);
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}