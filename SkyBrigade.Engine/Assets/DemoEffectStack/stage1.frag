layout(std140) uniform WaveStage
{
    float WaveStage_iTime;
};
/* This is added when the effect stage is loaded.
struct ShaderData {
	vec4 FragColor;
};
*/

ShaderData ShaderStage(ShaderData data)
{
    return ShaderData(data.FragColor * WaveStage_iTime);
}