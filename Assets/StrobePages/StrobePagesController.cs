using UnityEngine;

namespace StrobePages {

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages")]
public sealed partial class StrobePagesController : MonoBehaviour
{
    void OnDestroy() => ReleaseResources();

    void OnDisable() => ReleaseResources();

    void Update()
    {
        _phase += Time.deltaTime / PageInterval;

        if (_phase >= 1)
        {
            _phase -= (int)_phase;
            (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
            CaptureTarget = _pageFlip;
        }
        else
        {
            CaptureTarget = null;
        }
    }
}

} // namespace StrobePages
