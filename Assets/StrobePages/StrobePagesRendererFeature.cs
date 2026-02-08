using UnityEngine.Rendering.Universal;

sealed class StrobePagesRendererFeature : ScriptableRendererFeature
{
    StrobePagesPass _pass;

    public override void Create()
      => _pass = new StrobePagesPass
           { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
      => renderer.EnqueuePass(_pass);
}
