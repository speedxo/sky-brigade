using Horizon.OpenGL;
using Horizon.Rendering.Effects;

namespace SkyBrigade;

internal class FlashingEffect : Effect
{
    // A struct matching the shaders UBO.
    private struct FlashingStage
    {
        public float iTime { get; set; }
    }

    private FlashingStage data = default;

    // Providing shader source through the constructor.
    public FlashingEffect()
        : base(File.ReadAllText("Assets/DemoEffectStack/stage0.frag")) { }

    // Here we keep track of how much time has passed.
    public override void Update(float dt)
    {
        data.iTime += dt;
        RequiresUpdate = true;
    }

    // here we uplaod the data.
    public override void UpdateBuffer(in UniformBufferObject bufferObject)
    {
        bufferObject.BufferSingleData(data);
    }
}
