using ShaderType = Silk.NET.OpenGL.ShaderType;

namespace Horizon.Content
{
    public record struct ShaderDefinition(ShaderType Type, string File, string Source);
}
