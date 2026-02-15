using UnityEngine;

namespace StrobePages {

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("StrobePages/StrobePages Controller")]
public sealed partial class StrobePagesController : MonoBehaviour
{
    public void StartPageTurn()
    {
        if (_phase < 1) return;
        _phase -= (int)_phase;
        (_pageBase, _pageFlip) = (_pageFlip, _pageBase);
        _flags.capture = true;
    }

    void OnDestroy() => ReleaseResources();

    void OnDisable() => ReleaseResources();

    void Update()
    {
        _phase += Time.deltaTime / PageInterval;

        if (AutoPageTurn)
            StartPageTurn();
        else
            _phase = Mathf.Min(1, _phase);
    }
}

} // namespace StrobePages
