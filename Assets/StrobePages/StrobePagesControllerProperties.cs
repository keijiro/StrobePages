using UnityEngine;

public sealed partial class StrobePagesController
{
    #region Public properties

    [field:SerializeField, Range(0.02f, 0.5f)]
    public float PageInterval { get; set; } = 0.1f;

    [field:SerializeField, Range(1, 6)]
    public float EaseOutPower { get; set; } = 2;

    [field:SerializeField, Range(0, 0.5f)]
    public float MotionBlur { get; set; } = 0.07f;

    [field:SerializeField, Range(1, 5)]
    public float BackgroundScale { get; set; } = 3;

    [field:SerializeField, Range(1, 2)]
    public float ForegroundScale { get; set; } = 1;

    [field:SerializeField, Range(0, 1)]
    public float BackgroundOcclusionStrength { get; set; } = 0.85f;

    [field:SerializeField, Range(0.1f, 0.6f)]
    public float BackgroundOcclusionSize { get; set; } = 0.3333333f;

    [field:SerializeField, Range(0.05f, 0.5f)]
    public float BackgroundOcclusionExtent { get; set; } = 0.15f;

    [field:SerializeField, Range(0, 1)]
    public float ForegroundOcclusionStrength { get; set; }

    [field:SerializeField, Range(0.1f, 0.6f)]
    public float ForegroundOcclusionSize { get; set; } = 0.3333333f;

    [field:SerializeField, Range(0.05f, 0.5f)]
    public float ForegroundOcclusionExtent { get; set; } = 0.15f;

    [field:SerializeField]
    public Color BackgroundColor { get; set; } = Color.white;

    [field:SerializeField, HideInInspector]
    public Shader Shader { get; set; }

    public Material Material => UpdateMaterial();

    #endregion
}
