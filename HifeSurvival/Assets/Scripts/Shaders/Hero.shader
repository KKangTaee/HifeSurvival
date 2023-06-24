Shader "Heros"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        [KeywordEnum(Twist, Spawn)] _Status("Status", int) = 0

        //--------------
        // Spawn
        //--------------
        _Spawn_LightColor("Spawn_LightColor", Color) = (1,1,1,1)
        _Spawn_Val("Spawn_Val", Range(0,2)) = 0
    
    
        //--------------
        // Twist
        //--------------
        _TwistUvAmount("Twist Amount", Range(0, 3.1416)) = 1 //113
        _TwistUvRadius("Twist Radius", Range(0, 3)) = 0.75 //116
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Geometry"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {

            Name "Universal Forward"
            // Tags { "LightMode" = "UniversalForward"}

            Cull Off

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 

            //--------------
            // Variable
            //--------------

            CBUFFER_START(UnityPerMaterial)

            float4 _MainTex_ST; // tiling �� offset ����
            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            int _Status;


            float4 _Spawn_LightColor;
            float  _Spawn_Val;

            half _TwistUvAmount;
            half _TwistUvRadius;

            CBUFFER_END

            struct Attributes
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            half4 frag(Varyings o) : SV_Target
            {
                
                if (_Status == 0)
                {
                    half2 tiling = half2(0.5 * _MainTex_ST.x, 0.5 * _MainTex_ST.y);

                    half2 tempUv = o.uv - tiling;
                    _TwistUvRadius *= (_MainTex_ST.x + _MainTex_ST.y) / 2;

                    half percent = (_TwistUvRadius - length(tempUv)) / _TwistUvRadius;
                    half theta = percent * percent * (2.0 * sin(_TwistUvAmount)) * 8.0;
                    half s = sin(theta);
                    half c = cos(theta);
                    half beta = max(sign(_TwistUvRadius - length(tempUv)), 0.0);
                    tempUv = half2(dot(tempUv, half2(c, -s)), dot(tempUv, half2(s, c))) * beta + tempUv * (1 - beta);
                    tempUv += tiling;
                    o.uv = tempUv;

                    half4 col = _MainTex.Sample(sampler_MainTex, o.uv);

                    return col;
                }
                else if(_Status == 1)
                {
                    half4 col = _MainTex.Sample(sampler_MainTex, o.uv);

                    half spawnVal = clamp(_Spawn_Val, 0, 2);
                    half alpha = saturate((spawnVal - o.uv.y) * col.a);
                    half lerpVal = saturate(spawnVal - 1);

                    col = lerp(_Spawn_LightColor, col, lerpVal);
                    col.a = alpha;

                    return col;
                }
                else
                {
                    return half4(1,0,1,1);
                }
            }

            ENDHLSL
        }
    }
}