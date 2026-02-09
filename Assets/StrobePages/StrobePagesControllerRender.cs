using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using ShaderIDs = StrobePages.ShaderPropertyIDs;

namespace StrobePages {

public sealed partial class StrobePagesController
{
    [SerializeField, HideInInspector]
    Shader _shader = null;

    Material _material;
    RTHandle _pageBase;
    RTHandle _pageFlip;
    GraphicsFormat _bufferFormat;
    bool _needsInit = true;
    float _phase;

    public RTHandle CaptureTarget { get; private set; }

    public bool PrepareBuffers(GraphicsFormat format)
    {
        var formatChanged = format != _bufferFormat;

        if (_pageBase != null && !formatChanged) return false;

        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Base");
        _pageFlip = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Flip");

        _bufferFormat = format;

        _needsInit = true;
        _phase = 0;

        return true;
    }

    void ReleaseResources()
    {
        CoreUtils.Destroy(_material);
        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = null;
        _pageFlip = null;
    }

    public RTHandle ConsumeInitTarget()
    {
        if (!_needsInit || _pageBase == null) return null;
        _needsInit = false;
        return _pageBase;
    }

    public Material UpdateMaterial(float aspect)
    {
        if (_material == null) _material = CoreUtils.CreateEngineMaterial(_shader);

        _material.SetTexture(ShaderIDs.BaseTex, _pageBase.rt);
        _material.SetTexture(ShaderIDs.FlipTex, _pageFlip.rt);

        _material.SetFloat(ShaderIDs.Progress, _phase);
        _material.SetFloat(ShaderIDs.Blur, MotionBlur / 24 / PageInterval);
        _material.SetInt(ShaderIDs.SampleCount, Mathf.Clamp(SampleCount, 1, 32));
        _material.SetFloat(ShaderIDs.ShadeWidth, Mathf.Max(0.0001f, ShadeWidth));
        _material.SetFloat(ShaderIDs.ShadeStrength, Mathf.Max(0, ShadeStrength));
        _material.SetFloat(ShaderIDs.Stiffness, Mathf.Max(1, PageStiffness));

        return _material;
    }
}

} // namespace StrobePages
