using Silk.NET.OpenGL;

namespace Horizon.Content.Factories;

public record struct ShaderDefinition(ShaderType Type, string File, string Source);
