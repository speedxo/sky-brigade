using Horizon.OpenGL;

namespace Horizon.Rendering.Effects
{
    public class BasicEffect : Effect
    {
        public BasicEffect()
            : base(File.ReadAllText("Assets/effects/basicStage.frag")) { }

        public override void UpdateState(float dt) { }

        public override void UpdatePhysics(float dt) { }

        public override void UpdateBuffer(in UniformBufferObject bufferObject) { }
    }
}
