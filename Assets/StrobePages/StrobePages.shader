Shader "Hidden/StrobePages"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "StrobePages"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_BaseTex);
            SAMPLER(sampler_BaseTex);
            TEXTURE2D_X(_FlipTex);
            SAMPLER(sampler_FlipTex);

            float _Progress;
            float _Blur;
            float _Scale;

            float _Aspect;

            float3 SampleFlipPage(TEXTURE2D_PARAM(tex1, samp1),
                                  TEXTURE2D_PARAM(tex2, samp2),
                                  float2 uv, float t)
            {
                float y1 = uv.y - pow(saturate(1 - t), 2.2);
                float y2 = uv.y;
                float3 c1 = SAMPLE_TEXTURE2D_X(tex1, samp1, float2(uv.x, y1)).rgb;
                float3 c2 = SAMPLE_TEXTURE2D_X(tex2, samp2, float2(uv.x, y2)).rgb;
                return y1 > 0 ? c1 : c2;
            }

            float4 SampleFlipPageLayer(TEXTURE2D_PARAM(baseTex, baseSampler),
                                       TEXTURE2D_PARAM(flipTex, flipSampler),
                                       float2 uv, float progress, float blur,
                                       float aspect)
            {
                const uint SampleCount = 12;

                float3 acc = 0;
                if (blur > 0)
                {
                    float t0 = progress - blur / 2;
                    float dt = blur / SampleCount;
                    for (uint i = 0; i < SampleCount; i++)
                        acc += SampleFlipPage(TEXTURE2D_ARGS(baseTex, baseSampler),
                                              TEXTURE2D_ARGS(flipTex, flipSampler), uv, t0 + dt * i);
                    acc /= SampleCount;
                }
                else
                {
                    acc = SampleFlipPage(TEXTURE2D_ARGS(baseTex, baseSampler),
                                         TEXTURE2D_ARGS(flipTex, flipSampler), uv, progress);
                }

                return float4(acc, 1);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float2 uvPage = (uv - 0.5) / _Scale + 0.5;

                return SampleFlipPageLayer(TEXTURE2D_ARGS(_BaseTex, sampler_BaseTex),
                                           TEXTURE2D_ARGS(_FlipTex, sampler_FlipTex),
                                           uvPage, _Progress, _Blur, _Aspect);
            }
            ENDHLSL
        }
    }
}
