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
        var time = Time.timeAsDouble;
        var pageIndex = (int)(time / pageInterval);

        CaptureThisFrame = pageIndex != _lastCapturedPage;
        if (CaptureThisFrame) _lastCapturedPage = pageIndex;

        if (CaptureThisFrame && _pageBase != null && _pageFlip != null)
            (_pageBase, _pageFlip) = (_pageFlip, _pageBase);

        var t = time / pageInterval;
        var p = t - System.Math.Floor(t);

        var eased = 1 - System.Math.Pow(1 - p, EaseOutPower);
        var ddt = EaseOutPower * System.Math.Pow(1 - p, EaseOutPower - 1);

        Progress = (float)eased;
        Blur = MotionBlur * (float)ddt;
    }

    #endregion

    #region Controller implementation

    Material _material;
    RTHandle _pageBase;
    RTHandle _pageFlip;
    GraphicsFormat _bufferFormat;
    int _lastCapturedPage = int.MinValue;
    bool _needsInit = true;

    public bool CaptureThisFrame { get; private set; }
    public float Progress { get; private set; }
    public float Blur { get; private set; }

    public RTHandle CaptureTarget => CaptureThisFrame ? _pageFlip : null;
    public RTHandle PageBase => _pageBase;
    public RTHandle PageFlip => _pageFlip;

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
        _lastCapturedPage = int.MinValue;
        _needsInit = true;
    }

    public RTHandle ConsumeInitTarget()
    {
        if (!_needsInit || _pageBase == null) return null;
        _needsInit = false;
        return _pageBase;
    }

    public Material UpdateMaterial(float aspect)
    {
        if (_material == null)
        {
            _material = CoreUtils.CreateEngineMaterial(Shader);
        }

        _material.SetTexture(ShaderIDs.BaseTex, _pageBase.rt);
        _material.SetTexture(ShaderIDs.FlipTex, _pageFlip.rt);

        _material.SetFloat(ShaderIDs.Progress, Progress);
        _material.SetFloat(ShaderIDs.Blur, Blur);
        _material.SetInt(ShaderIDs.SampleCount, Mathf.Clamp(SampleCount, 1, 32));
        _material.SetFloat(ShaderIDs.ShadeWidth, Mathf.Max(0.0001f, ShadeWidth));
        _material.SetFloat(ShaderIDs.ShadeStrength, Mathf.Max(0, ShadeStrength));

        return _material;
    }

    #endregion
}

} // namespace StrobePages
