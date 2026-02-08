using UnityEngine;
using UnityEngine.Serialization;

public sealed partial class StrobePagesController
{
    #region Public properties

    [field:SerializeField, Range(0.02f, 0.5f)]
    public float PageInterval { get; set; } = 0.1f;

    [field:SerializeField, Range(1, 6)]
    public float EaseOutPower { get; set; } = 2;

    [field:SerializeField, Range(0, 0.5f)]
    public float MotionBlur { get; set; } = 0.07f;

    [field:SerializeField, Range(1, 32)]
    public int SampleCount { get; set; } = 12;

    [field:SerializeField, Range(0.01f, 1)]
    public float ShadeWidth { get; set; } = 0.3f;

    [field:SerializeField, Range(0, 8)]
    public float ShadeStrength { get; set; } = 4;

    [field:SerializeField, HideInInspector]
    public Shader Shader { get; set; }

    #endregion
}
