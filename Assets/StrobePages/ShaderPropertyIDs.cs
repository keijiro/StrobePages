using UnityEngine;

namespace StrobePages {

static class ShaderPropertyIDs
{
    public static readonly int BaseTex = Shader.PropertyToID("_BaseTex");
    public static readonly int FlipTex = Shader.PropertyToID("_FlipTex");
    public static readonly int Progress = Shader.PropertyToID("_Progress");
    public static readonly int Blur = Shader.PropertyToID("_Blur");
    public static readonly int SampleCount = Shader.PropertyToID("_SampleCount");
    public static readonly int ShadeWidth = Shader.PropertyToID("_ShadeWidth");
    public static readonly int ShadeStrength = Shader.PropertyToID("_ShadeStrength");
}

} // namespace StrobePages
