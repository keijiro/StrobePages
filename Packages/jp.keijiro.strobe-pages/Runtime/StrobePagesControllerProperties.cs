using UnityEngine;

namespace StrobePages {

public sealed partial class StrobePagesController
{
    #region Public properties

    [field:SerializeField]
    public bool AutoPageTurn { get; set; } = true;

    [field:SerializeField, Range(0.01f, 0.5f)]
    public float PageInterval { get; set; } = 0.1f;

    [field:SerializeField, Range(1, 10)]
    public float PageStiffness { get; set; } = 2;

    [field:SerializeField, Range(0, 1)]
    public float MotionBlur { get; set; } = 0.2f;

    [field:SerializeField, Range(1, 24)]
    public int SampleCount { get; set; } = 12;

    [field:SerializeField, Range(0.01f, 1)]
    public float ShadeWidth { get; set; } = 0.3f;

    [field:SerializeField, Range(0, 8)]
    public float ShadeStrength { get; set; } = 4;

    [field:SerializeField, Range(0, 1)]
    public float Opacity { get; set; } = 1;

    #endregion
}

} // namespace StrobePages
