using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace StrobePages {

sealed class StrobePagesPass : ScriptableRenderPass
{
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var camera = frameData.Get<UniversalCameraData>().camera;
        var controller = camera.GetComponent<StrobePagesController>();
        if (controller == null || !controller.enabled) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer) return;

        var source = resourceData.activeColorTexture;
        if (!source.IsValid()) return;

        var desc = renderGraph.GetTextureDesc(source);
        controller.PrepareBuffers(desc.format);

        var initTarget = controller.ConsumeInitTarget();
        if (initTarget != null)
        {
            var init = renderGraph.ImportTexture(initTarget);
            var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            var param = new RenderGraphUtils.BlitMaterialParameters(source, init, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages (Init)");
        }

        var captureTarget = controller.ConsumeCaptureTarget();
        if (captureTarget != null)
        {
            var capture = renderGraph.ImportTexture(captureTarget);
            var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            var param = new RenderGraphUtils.BlitMaterialParameters(source, capture, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages (Capture)");
        }

        desc.name = "_StrobePagesColor";
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var dest = renderGraph.CreateTexture(desc);

        var staticSource = controller.StaticSource;
        if (staticSource != null)
        {
            var page = renderGraph.ImportTexture(staticSource);
            var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
            var param = new RenderGraphUtils.BlitMaterialParameters(page, dest, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages (Static)");
        }
        else
        {
            var mat = controller.UpdateMaterial(camera.aspect);
            var param = new RenderGraphUtils.BlitMaterialParameters(source, dest, mat, 0);
            renderGraph.AddBlitPass(param, passName: "StrobePages");
        }

        resourceData.cameraColor = dest;
    }
}

} // namespace StrobePages
