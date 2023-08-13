layout(std140) uniform FlashingStage
{
    float FlashingStage_iTime;
};
/* This is added when the effect stage is loaded.
struct ShaderData {
	vec4 FragColor;
};
*/

ShaderData ShaderStage(ShaderData data)
{
    return ShaderData(data.FragColor * vec4(sin(FlashingStage_iTime), 1, 1, 1));
}