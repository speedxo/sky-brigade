using Horizon.OpenGL;
using Horizon.Rendering.Effects;

namespace Horizon.Prefabs.Effects
{
    public class VingetteEffect : Effect
    {
        public float Intensity
        {
            get => data.Intensity;
            set => data.Intensity = value;
        }

        private struct VignetteStage
        {
            public float Intensity { get; set; }

            public VignetteStage()
            {
                Intensity = 1.0f;
            }
        }

        private VignetteStage data = new VignetteStage();

        public VingetteEffect()
            : base(
                @"layout(std140) uniform VignetteStageData
{
    float Intensity;
} VignetteStage;

ShaderData ShaderStage(ShaderData data)
{
    // Calculate the distance from the center of the screen
    vec2 center = vec2(0.5, 0.5); // Assuming normalized coordinates
    float distance = length(texCoords - center);

    // Calculate the vignette intensity using a smooth curve
    float vignetteIntensity = smoothstep(0.4, 1.0, distance);

    return ShaderData(data.FragColor * vec4(vec3((1 - VignetteStage.Intensity * vignetteIntensity)), 1.0f), data.DepthComponent);
}
"
            )
        {
            RequiresUpdate = true;
        }

        public override void UpdateState(float dt)
        {
            RequiresUpdate = true;
            //data.Intensity = dt * 100.0f;
        }

        public override void UpdatePhysics(float dt) { }

        public override void UpdateBuffer(in UniformBufferObject bufferObject)
        {
            bufferObject.BufferSingleData(data);
        }
    }
}
