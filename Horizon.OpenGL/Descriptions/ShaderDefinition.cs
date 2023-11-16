using Horizon.Content.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Descriptions;

public record struct ShaderDefinition(ShaderType Type, string File, string Source);
