Shader "Unlit/TransparentWall"
{
    Properties
    {
         _MainTex("Texture", 2D) = "white" {}
         _StarPosition("Star Position", Vector) = (0, 0, 0, 0)
         _StarRadius("Star Radius", Range(0, 10)) = 1
         _Alpha("Alpha", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _StarPosition;
            float  _StarRadius;
            float  _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                 fixed4 col = tex2D(_MainTex, i.uv);

                // ���� ��ǥ�� ����Ͽ� ���� ��ġ�� ����մϴ�.
                float2 starWorldPosition = _StarPosition.xy;
                float2 worldUV = i.worldPos.xy;

                // ���� ��ǥ�� UV ��ǥ�� ��ȯ�մϴ�.
                float2 relativeUV = worldUV - starWorldPosition;

                // ���� ����ũ�� �Ÿ��� ����մϴ�.
                float distanceToStar = length(relativeUV);

                // ���� ����ũ ���� ������ ���İ��� �����մϴ�.
                if (distanceToStar < _StarRadius)
                {
                    if(col.a > 0.5f)
                       col.a = _Alpha;
                }

                return col;
            }
            ENDCG
        }
    }
}
