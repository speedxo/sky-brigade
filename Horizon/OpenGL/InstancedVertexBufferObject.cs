﻿using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/// <summary>
/// An abstraction around a vbo supporting a secondary buffer for instanced rendering.
/// </summary>
/// <typeparam name="VertexData">The vertex data.</typeparam>
/// <typeparam name="TInstancedData">The instancing data.</typeparam>
public class InstancedVertexBufferObject<VertexData, TInstancedData>
    : VertexBufferObject<VertexData>
    where VertexData : unmanaged
    where TInstancedData : unmanaged
{
    public BufferObject<TInstancedData> InstanceBuffer { get; init; }

    public InstancedVertexBufferObject()
        : base()
    {
        InstanceBuffer = new BufferObject<TInstancedData>(BufferTargetARB.ArrayBuffer);
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
