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

            TEXTURE2D_X(_BgBaseTex);
            SAMPLER(sampler_BgBaseTex);
            TEXTURE2D_X(_BgFlipTex);
            SAMPLER(sampler_BgFlipTex);
            TEXTURE2D_X(_FgBaseTex);
            SAMPLER(sampler_FgBaseTex);
            TEXTURE2D_X(_FgFlipTex);
            SAMPLER(sampler_FgFlipTex);

            float _BgProgress;
            float _BgBlur;
            float _BgScale;
            float3 _BgOcclusion;

            float _FgProgress;
            float _FgBlur;
            float _FgScale;
            float3 _FgOcclusion;

            float _Aspect;
            float4 _BackgroundColor;

            float FlipPageOcclusion(float2 uv, float size, float ext, float aspect)
            {
                float2 coord = max(0, abs(uv - 0.5) - size / 2);
                float dist = length(coord * float2(aspect, 1));
                return saturate(1 - dist / ext);
            }

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
                                       float3 occParams, float aspect)
            {
                const uint SampleCount = 12;

                float occ = FlipPageOcclusion(uv, occParams.x, occParams.y, aspect);

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

                float3 color = (1 - occ * occParams.z) * acc;
                float alpha = saturate(1 - length(max(abs(uv - 0.5) - 0.45, 0)) / 0.05);
                return float4(color, alpha);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float2 uvBg = (uv - 0.5) / _BgScale + 0.5;
                float2 uvFg = (uv - 0.5) / _FgScale + 0.5;

                float4 bg = SampleFlipPageLayer(TEXTURE2D_ARGS(_BgBaseTex, sampler_BgBaseTex),
                                                TEXTURE2D_ARGS(_BgFlipTex, sampler_BgFlipTex),
                                                uvBg, _BgProgress, _BgBlur, _BgOcclusion, _Aspect);

                float4 fg = SampleFlipPageLayer(TEXTURE2D_ARGS(_FgBaseTex, sampler_FgBaseTex),
                                                TEXTURE2D_ARGS(_FgFlipTex, sampler_FgFlipTex),
                                                uvFg, _FgProgress, _FgBlur, _FgOcclusion, _Aspect);

                float3 combined = lerp(bg.rgb, fg.rgb, fg.a);
                float alpha = max(bg.a, fg.a);
                float3 outColor = lerp(_BackgroundColor.rgb, combined, alpha);

                return float4(outColor, 1);
            }
            ENDHLSL
        }
    }
}
