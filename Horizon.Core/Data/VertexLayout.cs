using Silk.NET.OpenGL;

namespace Horizon.Core.Data;

/// <summary>
/// Attach this to properties and call Buffer.SetLayout() to automatically configure a buffer layout.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class VertexLayout : Attribute
{
    public uint Index { get; }
    public VertexAttribPointerType Type { get; }

    public VertexLayout(uint index, VertexAttribPointerType type)
    {
        this.Index = index;
        Type = type;
    }
}
