using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class StrobePagesPass : ScriptableRenderPass
{
    sealed class PassData
    {
        public TextureHandle destination;
        public Material material;
    }

    Material _material;
    RTHandle _captureTarget;
    RTHandle _initTarget;
    bool _captureThisFrame;
    ProfilingSampler _sampler;

    public void Setup(Material material,
                      RTHandle captureTarget,
                      bool captureThisFrame,
                      RTHandle initTarget)
    {
        _material = material;
        _captureTarget = captureTarget;
        _captureThisFrame = captureThisFrame;
        _initTarget = initTarget;
        _sampler ??= new ProfilingSampler("StrobePages");
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (_material == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        var source = resourceData.activeColorTexture;
        if (!source.IsValid()) return;

        if (_initTarget != null)
        {
            var init = renderGraph.ImportTexture(_initTarget);
            var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            var param = new RenderGraphUtils.BlitMaterialParameters(source, init, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages (Init)");
        }

        if (_captureThisFrame && _captureTarget != null)
        {
            var capture = renderGraph.ImportTexture(_captureTarget);
            var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            var param = new RenderGraphUtils.BlitMaterialParameters(source, capture, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages (Capture)");
        }

        var desc = renderGraph.GetTextureDesc(source);
        desc.name = "_StrobePagesColor";
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var destination = renderGraph.CreateTexture(desc);

        using var builder = renderGraph.AddRasterRenderPass<PassData>("StrobePages", out var passData, _sampler);
        passData.destination = destination;
        passData.material = _material;

        builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
        builder.AllowPassCulling(false);
        builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
        {
            CoreUtils.DrawFullScreen(context.cmd, data.material, null, 0);
        });

        resourceData.cameraColor = destination;
    }
}
