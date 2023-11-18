using Horizon.Core.Primitives;

namespace Horizon.OpenGL.Assets;

public class Shader : IGLObject
{
    public uint Handle { get; init; }

    public static Shader Invalid { get; } = new Shader { Handle = 0 };
}
