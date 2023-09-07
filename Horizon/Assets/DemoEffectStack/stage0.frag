layout(std140) uniform FlashingStageData { float iTime; }
FlashingStage;

ShaderData ShaderStage(ShaderData data) {
  return ShaderData(
      data.FragColor *
          vec4(abs(sin(FlashingStage.iTime * 10.0f) * 2.0f), 1.0f, 1.0f, 1.0f),
      data.DepthComponent);
}