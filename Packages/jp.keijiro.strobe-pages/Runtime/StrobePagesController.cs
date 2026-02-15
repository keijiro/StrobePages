using UnityEngine;

namespace StrobePages {

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages")]
public sealed partial class StrobePagesController : MonoBehaviour
{
    public void StartPageTurn()
    {
        if (AutoPageTurn) return;
        if (_isPageTurnActive) return;
        _isPageTurnActive = true;
    }

    void OnDestroy() => ReleaseResources();

    void OnDisable() => ReleaseResources();

    void Update()
    {
        if (AutoPageTurn) _isPageTurnActive = true;

        if (!_isPageTurnActive)
        {
            if (_phase < 0) _phase = 0;
            CaptureTarget = null;
            return;
        }

        _phase += Time.deltaTime / PageInterval;

        if (_phase >= 1)
        {
            _phase -= (int)_phase;
            (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
            CaptureTarget = _pageFlip;
            if (!AutoPageTurn) _isPageTurnActive = false;
        }
        else
        {
            CaptureTarget = null;
        }
    }
}

} // namespace StrobePages
