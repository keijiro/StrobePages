using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

sealed class StrobePagesRendererFeature : ScriptableRendererFeature
{
    static readonly int BgBaseTexId = Shader.PropertyToID("_BgBaseTex");
    static readonly int BgFlipTexId = Shader.PropertyToID("_BgFlipTex");
    static readonly int BgProgressId = Shader.PropertyToID("_BgProgress");
    static readonly int BgBlurId = Shader.PropertyToID("_BgBlur");

    static readonly int FgBaseTexId = Shader.PropertyToID("_FgBaseTex");
    static readonly int FgFlipTexId = Shader.PropertyToID("_FgFlipTex");
    static readonly int FgProgressId = Shader.PropertyToID("_FgProgress");
    static readonly int FgBlurId = Shader.PropertyToID("_FgBlur");

    static readonly int AspectId = Shader.PropertyToID("_Aspect");

    StrobePagesPass _pass;
    RTHandle _pageBase;
    RTHandle _pageFlip;
    int _lastCapturedPage = int.MinValue;
    int _historyWidth;
    int _historyHeight;
    GraphicsFormat _historyFormat;
    bool _needsInit;

    public override void Create()
    {
        _pass = new StrobePagesPass
          { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var camera = renderingData.cameraData.camera;
        var controller = camera.GetComponent<StrobePagesController>();
        if (controller == null || !controller.enabled) return;

        var material = controller.Material;
        if (material == null) return;

        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;
        descriptor.useMipMap = false;
        descriptor.autoGenerateMips = false;
        descriptor.enableRandomWrite = false;

        var sizeChanged = descriptor.width != _historyWidth ||
                          descriptor.height != _historyHeight ||
                          descriptor.graphicsFormat != _historyFormat;

        _historyWidth = descriptor.width;
        _historyHeight = descriptor.height;
        _historyFormat = descriptor.graphicsFormat;

        if (_pageBase == null || sizeChanged)
        {
            _pageBase?.Release();
            _pageFlip?.Release();
            _pageBase = null;
            _pageFlip = null;
            _needsInit = true;
        }

        RenderingUtils.ReAllocateHandleIfNeeded(ref _pageBase, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_StrobePages_Base");
        RenderingUtils.ReAllocateHandleIfNeeded(ref _pageFlip, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_StrobePages_Flip");

        var pageInterval = Mathf.Max(0.01f, controller.PageInterval);
        var time = Time.timeAsDouble;
        var pageIndex = (int)(time / pageInterval);
        var captureThisFrame = pageIndex != _lastCapturedPage;

        if (captureThisFrame)
        {
            _lastCapturedPage = pageIndex;
            (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
        }

        var (bgProgress, fgProgress, bgBlur, fgBlur) =
            ResolveProgress(pageInterval, time, controller.EaseOutPower, controller.MotionBlur);

        UpdateMaterialProperties(material, descriptor,
                                 _pageBase, _pageFlip,
                                 bgProgress, fgProgress, bgBlur, fgBlur);

        var initTarget = _needsInit ? _pageBase : null;
        _needsInit = false;

        _pass.Setup(material, captureThisFrame ? _pageFlip : null, captureThisFrame, initTarget);
        renderer.EnqueuePass(_pass);
    }

    [Obsolete]
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (_pass == null) return;
        _pass.ConfigureInput(ScriptableRenderPassInput.Color);
    }

    static (float bgProgress, float fgProgress, float bgBlur, float fgBlur)
        ResolveProgress(float pageInterval, double time, float easeOutPower, float motionBlur)
    {
        var t = time / pageInterval;
        var p = t - Math.Floor(t);

        var eased = 1 - Math.Pow(1 - p, easeOutPower);
        var ddt = easeOutPower * Math.Pow(1 - p, easeOutPower - 1);

        var bgProgress = (float)p;
        var fgProgress = (float)eased;

        var bgBlur = motionBlur;
        var fgBlur = motionBlur * (float)ddt;

        return (bgProgress, fgProgress, bgBlur, fgBlur);
    }

    void UpdateMaterialProperties(Material material,
                                  RenderTextureDescriptor descriptor,
                                  RTHandle pageBase, RTHandle pageFlip,
                                  float bgProgress, float fgProgress,
                                  float bgBlur, float fgBlur)
    {
        material.SetTexture(BgBaseTexId, pageBase.rt);
        material.SetTexture(BgFlipTexId, pageFlip.rt);
        material.SetTexture(FgBaseTexId, pageBase.rt);
        material.SetTexture(FgFlipTexId, pageFlip.rt);

        material.SetFloat(BgProgressId, bgProgress);
        material.SetFloat(FgProgressId, fgProgress);
        material.SetFloat(BgBlurId, bgBlur);
        material.SetFloat(FgBlurId, fgBlur);

        var aspect = descriptor.width / (float)Math.Max(1, descriptor.height);
        material.SetFloat(AspectId, aspect);
    }

    protected override void Dispose(bool disposing)
    {
        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = null;
        _pageFlip = null;
    }
}
