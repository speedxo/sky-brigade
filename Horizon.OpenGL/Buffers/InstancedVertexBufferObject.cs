using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.OpenGL.Buffers;

/// <summary>
/// An extension around <see cref="VertexBufferObject"/> adding a secondary buffer for instanced rendering.
/// </summary>
/// <typeparam name="VertexData">The vertex data.</typeparam>
/// <typeparam name="TInstancedData">The instancing data.</typeparam>
public class InstancedVertexBufferObject
    : VertexBufferObject
{
    public BufferObject InstanceBuffer { get; init; }

    public InstancedVertexBufferObject()
        : base()
    {
        InstanceBuffer = new BufferObject();
    }

    public override void Bind()
    {
        base.Bind();
        InstanceBuffer.Bind();
    }

    public override void Unbind()
    {
        base.Unbind();
        InstanceBuffer.Unbind();
    }

    public override void Dispose()
    {
        base.Dispose();
        InstanceBuffer.Dispose();
    }
}
