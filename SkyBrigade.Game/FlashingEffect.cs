using Horizon.OpenGL;
using Horizon.Rendering.Effects;

namespace SkyBrigade;

internal class FlashingEffect : Effect
{
    private struct FlashingStage
    {
        public float FlashingStage_iTime { get; set; }
    }

    private FlashingStage data = default;

    public FlashingEffect() :
        base(File.ReadAllText("Assets/DemoEffectStack/stage0.frag"))
    {
    }


    public override void UpdateBuffer(float dt, in UniformBufferObject bufferObject)
    {
        data.FlashingStage_iTime += dt;

        bufferObject.BufferSingleData(data);
    }
}
