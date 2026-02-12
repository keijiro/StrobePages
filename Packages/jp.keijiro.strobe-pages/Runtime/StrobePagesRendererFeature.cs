using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StrobePages {

sealed class StrobePagesRendererFeature : ScriptableRendererFeature
{
    [SerializeField] RenderPassEvent _passEvent =
      RenderPassEvent.BeforeRenderingPostProcessing;

    StrobePagesPass _pass;

    public override void Create()
      => _pass = new StrobePagesPass { renderPassEvent = _passEvent };

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData renderingData)
      => renderer.EnqueuePass(_pass);
}

} // namespace StrobePages
