using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Horizon.Content;
using Horizon.Core.Data;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;

using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Buffers;

//The vertex array object abstraction.
public class VertexBufferObject
{
    public uint Handle
    {
        get => VertexArrayObject.Handle;
    }

    public BufferObject VertexBuffer { get; init; }
    public BufferObject ElementBuffer { get; init; }

    public BufferObject? InstanceBuffer { get; init; }
    public BufferObject? IndirectBuffer { get; init; }

    public VertexArrayObject VertexArrayObject { get; init; }

    public VertexBufferObject(in VertexArrayObject vao)
    {
        this.VertexArrayObject = vao;

        VertexBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer];
        ElementBuffer = vao.Buffers[VertexArrayBufferAttachmentType.ElementBuffer];

        {
            if (vao.Buffers.TryGetValue(VertexArrayBufferAttachmentType.IndirectBuffer, out BufferObject? value))
                IndirectBuffer = value;
        }
        {
            if (vao.Buffers.TryGetValue(VertexArrayBufferAttachmentType.AdditionalBuffer0, out BufferObject? value))
                InstanceBuffer = value;
        }

        Bind();
        VertexBuffer.Bind();
        Unbind();
    }

    public VertexBufferObject(AssetCreationResult<VertexArrayObject> result)
        : this(result.Asset) { }


    public virtual void Bind()
    {
        // Binding the vertex array.
        ObjectManager.GL.BindVertexArray(Handle);
        VertexBuffer.Bind();
        ElementBuffer.Bind();
    }

    public virtual void Unbind()
    {
        // Unbinding the vertex array.
        ObjectManager.GL.BindVertexArray(0);
        VertexBuffer.Unbind();
        ElementBuffer.Unbind();
    }
}
