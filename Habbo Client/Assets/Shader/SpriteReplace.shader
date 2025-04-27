// Shader URP Shader Graph equivalent (HLSL base for understanding or fallback)
// Replaces indexed palette from a source texture with colors from a target texture

Shader "Custom/URP_PaletteReplace"
{
    Properties
    {
        [MainTexture]_MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteSource ("Palette Source", 2D) = "white" {}
        _PaletteTarget ("Palette Target", 2D) = "white" {}
        _Tolerance ("Color Match Tolerance", Range(0,1)) = 0.01
        _MaxDistance ("Maximum Match Distance", Range(0,1)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _PaletteSource;
            sampler2D _PaletteTarget;
            float _Tolerance;
            float _MaxDistance;

            #define PALETTE_COUNT 10

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            float ColorDistance(float3 a, float3 b)
            {
                return distance(a, b);
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.texcoord);

                float minDist = 9999.0;
                int bestIndex = -1;

                for (int x = 0; x < PALETTE_COUNT; x++)
                {
                    float u = (x + 0.5) / PALETTE_COUNT;
                    float3 sourceColorVec = tex2D(_PaletteSource, float2(u, 0)).rgb;
                    float sourceB = sourceColorVec.b;

                    if (abs(col.b - sourceB) < _Tolerance  && col.r <= 0.1 && col.b > 0.01)
                    {
                        float dist = ColorDistance(col, sourceColorVec);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            bestIndex = x;
                        }
                    }
                }

                if (bestIndex >= 0 && minDist <= _MaxDistance)
                {
                    float u = (bestIndex + 0.5) / PALETTE_COUNT;
                    float3 targetColor = tex2D(_PaletteTarget, float2(u, 0)).rgb;
                    return float4(targetColor, col.a);
                }

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}