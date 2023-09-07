layout(std140) uniform BasicStage { float iTime; };

ShaderData ShaderStage(ShaderData data) {
  return ShaderData(data.FragColor, data.DepthComponent);
}