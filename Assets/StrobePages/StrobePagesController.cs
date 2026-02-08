using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages/Strobe Pages")]
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

    sealed class ShaderToken
    {
        public readonly int BaseTex = Shader.PropertyToID("_BaseTex");
        public readonly int FlipTex = Shader.PropertyToID("_FlipTex");
        public readonly int Progress = Shader.PropertyToID("_Progress");
        public readonly int Blur = Shader.PropertyToID("_Blur");

        public readonly int Aspect = Shader.PropertyToID("_Aspect");
    }

    ShaderToken _token;
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
        if (Shader == null) Shader = Shader.Find("Hidden/StrobePages");
        if (Shader == null) return null;

        if (_material == null || _material.shader != Shader)
        {
            _token = new ShaderToken();
            _material = CoreUtils.CreateEngineMaterial(Shader);
        }

        _material.SetTexture(_token.BaseTex, _pageBase.rt);
        _material.SetTexture(_token.FlipTex, _pageFlip.rt);

        _material.SetFloat(_token.Progress, Progress);
        _material.SetFloat(_token.Blur, Blur);

        _material.SetFloat(_token.Aspect, aspect);

        return _material;
    }

    #endregion
}
