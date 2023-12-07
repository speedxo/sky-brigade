using System.Runtime.InteropServices;
using AutoVoxel.Data;
using Horizon.Core.Primitives;
using Horizon.Engine;
using Horizon.OpenGL.Buffers;

using Silk.NET.OpenGL;

namespace AutoVoxel.Rendering;

/// <summary>
/// Class to simplify storing a voxel mesh.
/// </summary>
public class TessellatorMesh : IDisposable, IRenderable
{
    public uint ElementCount { get; protected set; }
    public VertexBufferObject Buffer { get; protected set; }

    public readonly List<ChunkVertex> Vertices = new();
    public readonly List<uint> Indices = new();
    private bool isDirty;

    public void Clear()
    {
        Vertices.Clear();
        Indices.Clear();
    }

    protected void SetBufferLayout()
    {
        Buffer.Bind();
        Buffer.VertexBuffer.Bind();
        Buffer.SetLayout<ChunkVertex>();
        Buffer.VertexBuffer.Unbind();
        Buffer.Unbind();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ChunkManager.BufferPool.Return(Buffer);
    }

    public unsafe void Render(float dt, object? obj = null)
    {
        Buffer ??= ChunkManager.BufferPool.Get();

        if (isDirty)
        {
            isDirty = false;

            if (Indices.Count < ElementCount) // don't reallocate unless we have to.
            {
                Buffer.VertexBuffer.NamedBufferSubData((ReadOnlySpan<ChunkVertex>)CollectionsMarshal.AsSpan(Vertices));
                Buffer.ElementBuffer.NamedBufferSubData((ReadOnlySpan<uint>)CollectionsMarshal.AsSpan(Indices));
            }
            else
            {
                Buffer.VertexBuffer.NamedBufferData((ReadOnlySpan<ChunkVertex>)CollectionsMarshal.AsSpan(Vertices));
                Buffer.ElementBuffer.NamedBufferData((ReadOnlySpan<uint>)CollectionsMarshal.AsSpan(Indices));
            }

            ElementCount = (uint)Indices.Count;
            Clear();
        }

        if (ElementCount < 1)
            return;

        Buffer.Bind();
        GameEngine
            .Instance
            .GL
            .DrawElements(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null
            );
    }

    public void FlagDirty()
    {
        isDirty = true;
    }
}
