using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering.Effects;

namespace SkyBrigade.Game;

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
