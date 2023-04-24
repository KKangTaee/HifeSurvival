Shader "Unlit/MaskLoading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Tex", 2D) = "white" {}
        _MaskValue ("Mask Value", Range(0, 15)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
                float2 uv      : TEXCOORD0;
                float2 mask_uv : TEXCOORD1;
                float4 vertex  : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            float  _MaskValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 centeredUV = v.uv - 0.5;
                centeredUV *=  (_MaskValue * float2(1, 1));
                centeredUV += 0.5 + float2(0, 0);
                o.mask_uv = centeredUV;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col  = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.mask_uv);

                mask = 1 - mask;
                col.a = mask.r;

                return  col;
            }
            ENDCG
        }
    }
}