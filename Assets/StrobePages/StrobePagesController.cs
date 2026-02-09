using UnityEngine;
using UnityEngine.Rendering;

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
        CaptureThisFrame = (_phase >= 1);

        if (CaptureThisFrame)
        {
            _phase -= (int)_phase;
            (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
        }
    }
}

} // namespace StrobePages
