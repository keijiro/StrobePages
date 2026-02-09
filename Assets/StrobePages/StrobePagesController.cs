using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using ShaderIDs = StrobePages.ShaderPropertyIDs;

namespace StrobePages {

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages")]
public sealed partial class StrobePagesController : MonoBehaviour
{
    #region MonoBehaviour implementation

    void OnDestroy()
    {
        CoreUtils.Destroy(_material);
        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = null;
        _pageFlip = null;
    }

    void OnDisable()
      => OnDestroy();

    void Update()
    {
        var pageInterval = Mathf.Max(0.01f, PageInterval);
        _phase += Time.deltaTime / pageInterval;

        var pageStep = Mathf.FloorToInt(_phase);
        if (pageStep > 0)
        {
            _phase -= pageStep;
            CaptureThisFrame = true;

            if (_pageBase != null && _pageFlip != null && (pageStep & 1) == 1)
                (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
        }
        else
        {
            CaptureThisFrame = false;
        }

        Progress = _phase;
        Blur = MotionBlur;
    }

    #endregion

    #region Controller implementation

    Material _material;
    RTHandle _pageBase;
    RTHandle _pageFlip;
    GraphicsFormat _bufferFormat;
    bool _needsInit = true;
    float _phase;

    public bool CaptureThisFrame { get; private set; }
    public float Progress { get; private set; }
    public float Blur { get; private set; }

    public RTHandle CaptureTarget => CaptureThisFrame ? _pageFlip : null;

    public bool PrepareBuffers(GraphicsFormat format)
    {
        var formatChanged = format != _bufferFormat;

        if (_pageBase != null && !formatChanged) return false;

        _pageBase?.Release();
        _pageFlip?.Release();
        _pageBase = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Base");
        _pageFlip = RTHandles.Alloc(Vector3.one, format, name: "_StrobePages_Flip");

        _bufferFormat = format;

        ResetState();
        return true;
    }

    public void ResetState()
    {
        _needsInit = true;
        _phase = 0;
    }

    public RTHandle ConsumeInitTarget()
    {
        if (!_needsInit || _pageBase == null) return null;
        _needsInit = false;
        return _pageBase;
    }

    public Material UpdateMaterial(float aspect)
    {
        if (_material == null) _material = CoreUtils.CreateEngineMaterial(Shader);

        _material.SetTexture(ShaderIDs.BaseTex, _pageBase.rt);
        _material.SetTexture(ShaderIDs.FlipTex, _pageFlip.rt);

        _material.SetFloat(ShaderIDs.Progress, Progress);
        _material.SetFloat(ShaderIDs.Blur, Blur);
        _material.SetInt(ShaderIDs.SampleCount, Mathf.Clamp(SampleCount, 1, 32));
        _material.SetFloat(ShaderIDs.ShadeWidth, Mathf.Max(0.0001f, ShadeWidth));
        _material.SetFloat(ShaderIDs.ShadeStrength, Mathf.Max(0, ShadeStrength));
        _material.SetFloat(ShaderIDs.Stiffness, Mathf.Max(1, PageStiffness));

        return _material;
    }

    #endregion
}

} // namespace StrobePages
