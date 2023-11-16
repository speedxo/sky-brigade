namespace Horizon.Core.Primitives;

/// <summary>
/// The most primitive type, represents any handle bound object.
/// </summary>
public interface IGLObject
{
    public uint Handle { get; init; }
}

/// <summary>
/// Abstraction around <see href="IGLObject">, providing a GL context set at runtime by GameEngine.
/// </summary>
public abstract class GLObject : IGLObject
{
    public static void SetGL(in Silk.NET.OpenGL.GL gl) => GL ??= gl;

    protected static Silk.NET.OpenGL.GL GL { get; private set; }
    public uint Handle { get; init; }
}
