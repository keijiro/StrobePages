Shader "Hidden/StrobePages"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D_X(_BaseTex);
TEXTURE2D_X(_FlipTex);
SAMPLER(sampler_BaseTex);
SAMPLER(sampler_FlipTex);

float _Progress;
float _Blur;
int _SampleCount;
float _ShadeWidth;
float _ShadeStrength;

static const int MaxSamples = 24;

float4 Frag(Varyings input) : SV_Target
{
    float2 uv = input.texcoord;

    int sampleCount = min(max(_SampleCount, 1), MaxSamples);

    float3 acc = 0;
    float t0 = _Progress * (1 + _Blur) - _Blur;
    float dt = _Blur / sampleCount;

    [loop]
    for (int i = 0; i < MaxSamples; i++)
    {
        if (i >= sampleCount) break;
        float t = t0 + dt * i;
        float fall = pow(saturate(1 - t), 2.2);
        float y1 = uv.y - fall;
        float3 c1 = SAMPLE_TEXTURE2D_X(_BaseTex, sampler_BaseTex, float2(uv.x, y1)).rgb;
        float3 c2 = SAMPLE_TEXTURE2D_X(_FlipTex, sampler_FlipTex, uv).rgb;
        float shade = lerp(1, saturate(-y1 / _ShadeWidth), saturate(_ShadeStrength * fall));
        acc += y1 > 0 ? c1 : c2 * shade;
    }

    return float4(acc / sampleCount, 1);
}

ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "StrobePages"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
