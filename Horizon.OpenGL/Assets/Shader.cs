using Horizon.Core.Primitives;

namespace Horizon.OpenGL.Assets;

public readonly struct Shader : IGLObject
{
    public readonly uint Handle { get; init; }

    public static Shader Invalid { get; } = new Shader { Handle = 0 };
}
