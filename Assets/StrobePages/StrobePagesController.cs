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

        BgProgress = (float)p;
        FgProgress = (float)eased;
        BgBlur = MotionBlur;
        FgBlur = MotionBlur * (float)ddt;
    }

    #endregion

    #region Controller implementation

    sealed class ShaderToken
    {
        public readonly int BgScale = Shader.PropertyToID("_BgScale");
        public readonly int FgScale = Shader.PropertyToID("_FgScale");
        public readonly int BgOcclusion = Shader.PropertyToID("_BgOcclusion");
        public readonly int FgOcclusion = Shader.PropertyToID("_FgOcclusion");
        public readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");

        public readonly int BgBaseTex = Shader.PropertyToID("_BgBaseTex");
        public readonly int BgFlipTex = Shader.PropertyToID("_BgFlipTex");
        public readonly int BgProgress = Shader.PropertyToID("_BgProgress");
        public readonly int BgBlur = Shader.PropertyToID("_BgBlur");

        public readonly int FgBaseTex = Shader.PropertyToID("_FgBaseTex");
        public readonly int FgFlipTex = Shader.PropertyToID("_FgFlipTex");
        public readonly int FgProgress = Shader.PropertyToID("_FgProgress");
        public readonly int FgBlur = Shader.PropertyToID("_FgBlur");

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
    public float BgProgress { get; private set; }
    public float FgProgress { get; private set; }
    public float BgBlur { get; private set; }
    public float FgBlur { get; private set; }

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

        _material.SetFloat(_token.BgScale, Mathf.Max(0.1f, BackgroundScale));
        _material.SetFloat(_token.FgScale, Mathf.Max(0.1f, ForegroundScale));

        _material.SetVector(_token.BgOcclusion, new Vector4(BackgroundOcclusionSize,
                                                           BackgroundOcclusionExtent,
                                                           BackgroundOcclusionStrength, 0));

        _material.SetVector(_token.FgOcclusion, new Vector4(ForegroundOcclusionSize,
                                                           ForegroundOcclusionExtent,
                                                           ForegroundOcclusionStrength, 0));

        _material.SetColor(_token.BackgroundColor, BackgroundColor);

        _material.SetTexture(_token.BgBaseTex, _pageBase.rt);
        _material.SetTexture(_token.BgFlipTex, _pageFlip.rt);
        _material.SetTexture(_token.FgBaseTex, _pageBase.rt);
        _material.SetTexture(_token.FgFlipTex, _pageFlip.rt);

        _material.SetFloat(_token.BgProgress, BgProgress);
        _material.SetFloat(_token.FgProgress, FgProgress);
        _material.SetFloat(_token.BgBlur, BgBlur);
        _material.SetFloat(_token.FgBlur, FgBlur);

        _material.SetFloat(_token.Aspect, aspect);

        return _material;
    }

    #endregion
}
