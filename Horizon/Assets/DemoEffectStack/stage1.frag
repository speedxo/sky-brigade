layout(std140) uniform WaveStage
{
    float WaveStage_iTime;
};

ShaderData ShaderStage(ShaderData data)
{
    return ShaderData(data.FragColor, data.DepthComponent);
}