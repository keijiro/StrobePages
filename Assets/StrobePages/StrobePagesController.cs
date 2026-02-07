using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages/Strobe Pages")]
public sealed partial class StrobePagesController : MonoBehaviour
{
    #region MonoBehaviour implementation

    void OnDestroy()
      => CoreUtils.Destroy(_material);

    void OnDisable()
      => OnDestroy();

    void Update() {} // Just for providing the component enable switch.

    #endregion

    #region Controller implementation

    sealed class ShaderToken
    {
        public readonly int BgScale = Shader.PropertyToID("_BgScale");
        public readonly int FgScale = Shader.PropertyToID("_FgScale");
        public readonly int BgOcclusion = Shader.PropertyToID("_BgOcclusion");
        public readonly int FgOcclusion = Shader.PropertyToID("_FgOcclusion");
        public readonly int BackgroundColor = Shader.PropertyToID("_BackgroundColor");
    }

    ShaderToken _token;
    Material _material;

    public Material UpdateMaterial()
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

        return _material;
    }

    #endregion
}
