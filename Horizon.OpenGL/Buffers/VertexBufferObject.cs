using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Content;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Buffers;

//The vertex array object abstraction.
public class VertexBufferObject
{
    public uint Handle
    {
        get => vao.Handle;
    }

    public BufferObject VertexBuffer { get; init; }
    public BufferObject ElementBuffer { get; init; }
    public BufferObject? InstanceBuffer { get; init; }

    private VertexArrayObject vao;

    public VertexBufferObject(in VertexArrayObject vao)
    {
        this.vao = vao;

        VertexBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer];
        ElementBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ElementBuffer];

        if (vao.Buffers.ContainsKey(VertexArrayBufferAttachmentType.InstanceBuffer))
            InstanceBuffer = vao.Buffers[VertexArrayBufferAttachmentType.InstanceBuffer];

        Bind();
        VertexBuffer.Bind();
        Unbind();
    }

    public VertexBufferObject(AssetCreationResult<VertexArrayObject> result)
        : this(result.Asset) { }

    public unsafe void VertexAttributePointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        int offSet
    )
    {
        ContentManager
            .GL
            .VertexAttribPointer(index, count, type, false, vertexSize, (void*)(offSet));
        ContentManager.GL.EnableVertexAttribArray(index);
    }

    public void VertexAttributeDivisor(uint index, uint divisor)
    {
        ContentManager.GL.VertexAttribDivisor(index, divisor);
    }

    public virtual void Bind()
    {
        // Binding the vertex array.
        ContentManager.GL.BindVertexArray(Handle);
    }

    public virtual void Unbind()
    {
        // Unbinding the vertex array.
        ContentManager.GL.BindVertexArray(0);
    }
}
