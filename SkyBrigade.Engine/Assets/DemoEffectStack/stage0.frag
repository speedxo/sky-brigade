layout(std140) uniform FlashingStage
{
    float FlashingStage_iTime;
};

ShaderData ShaderStage(ShaderData data)
{
    return ShaderData(data.FragColor * vec4(abs(sin(FlashingStage_iTime * 10.0f) * 2.0f), 1.0f, 1.0f, 1.0f), data.DepthComponent);
}