using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using ShaderIDs = StrobePages.ShaderPropertyIDs;

namespace StrobePages {

public sealed partial class StrobePagesController
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    RTHandle _pageBase;
    RTHandle _pageFlip;
    (bool init, bool capture) _flags;
    float _phase;

    void ReleaseResources()
    {
        CoreUtils.Destroy(_material);
        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = null;
        _pageFlip = null;
    }

    public void PrepareBuffers(GraphicsFormat format)
    {
        if (_pageBase != null) return;
        _pageBase = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Base");
        _pageFlip = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Flip");
        _flags = (true, true);
    }

    public RTHandle ConsumeInitTarget()
    {
        if (!_flags.init) return null;
        _flags.init = false;
        return _pageBase;
    }

    public RTHandle ConsumeCaptureTarget()
    {
        if (!_flags.capture) return null;
        _flags.capture = false;
        return _pageFlip;
    }

    public RTHandle StaticSource
      => !AutoPageTurn && _phase >= 1 ? _pageFlip : null;

    public Material UpdateMaterial(float aspect)
    {
        if (_material == null) _material = CoreUtils.CreateEngineMaterial(_shader);

        _material.SetTexture(ShaderIDs.BaseTex, _pageBase.rt);
        _material.SetTexture(ShaderIDs.FlipTex, _pageFlip.rt);
        _material.SetFloat(ShaderIDs.Progress, Mathf.Clamp01(_phase));
        _material.SetFloat(ShaderIDs.Blur, MotionBlur / 24 / PageInterval);
        _material.SetInt(ShaderIDs.SampleCount, Mathf.Clamp(SampleCount, 1, 32));
        _material.SetFloat(ShaderIDs.ShadeWidth, Mathf.Max(0.0001f, ShadeWidth));
        _material.SetFloat(ShaderIDs.ShadeStrength, Mathf.Max(0, ShadeStrength));
        _material.SetFloat(ShaderIDs.Stiffness, Mathf.Max(1, PageStiffness));
        _material.SetFloat(ShaderIDs.Opacity, Mathf.Clamp01(Opacity));

        return _material;
    }
}

} // namespace StrobePages
