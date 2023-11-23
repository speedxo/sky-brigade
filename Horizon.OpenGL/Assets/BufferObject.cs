using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Primitives;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Assets;

/* This is an abstraction for a buffer object */
public class BufferObject : GLObject
{
    public BufferTargetARB Type { get; init; }

    public static long ALIGNMENT = 0;

    static BufferObject()
    {
        ALIGNMENT = GL.GetInteger64(GetPName.MinMapBufferAlignment);
    }

    public virtual unsafe void BufferData<T>(in ReadOnlySpan<T> data)
        where T : unmanaged
    {
        Bind();
        // FIXME cross static ref to BaseGameEngine

        GL.BufferData(Type, (nuint)(data.Length * sizeof(T)), data, BufferUsageARB.DynamicDraw);

        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferSubData<T>(in ReadOnlySpan<T> data, int offset = 0)
        where T : unmanaged
    {
        Bind();

        // FIXME cross static ref to BaseGameEngine
        GL.BufferSubData(Type, offset, (nuint)(sizeof(T) * data.Length), data);

        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferSubData<T>(in T[] data, int offset = 0)
        where T : unmanaged
    {
        Bind();

        fixed (void* d = data)
        {
            // FIXME cross static ref to BaseGameEngine
            GL.BufferSubData(Type, offset, (nuint)(sizeof(T) * data.Length), d);
        }

        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferData<T>(in T[] data)
        where T : unmanaged
    {
        Bind();

        fixed (void* d = data)
        {
            // FIXME cross static ref to BaseGameEngine
            GL.BufferData(Type, (nuint)(data.Length * sizeof(T)), d, BufferUsageARB.DynamicDraw);
        }
        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, 0);
    }

    public virtual void Bind()
    {
        /* Binding the buffer object, with the correct buffer type.
         */
        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, Handle);
    }

    public virtual unsafe void NamedBufferData<T>(in ReadOnlySpan<T> data)
        where T : unmanaged
    {
        // FIXME cross static ref to BaseGameEngine
        GL.NamedBufferData(
            Handle,
            (nuint)(data.Length * sizeof(T)),
            data,
            VertexBufferObjectUsage.DynamicDraw
        );
    }

    public virtual unsafe void NamedBufferSubData<T>(in ReadOnlySpan<T> data, int offset = 0, int length = 0)
        where T : unmanaged
    {
        // FIXME cross static ref to BaseGameEngine
        GL.NamedBufferSubData(Handle, offset, (nuint)(length > 0 ? length : (sizeof(T) * data.Length)), data);
    }

    public virtual unsafe void NamedBufferSubData<T>(in T[] data, int offset = 0, int length = 0)
        where T : unmanaged
    {
        fixed (void* d = data)
        {
            GL.NamedBufferData(
                Handle,
                (nuint)(length > 0 ? length : (sizeof(T) * data.Length)),
                d,
                VertexBufferObjectUsage.DynamicDraw
            );
        }
    }

    public virtual unsafe void NamedBufferData<T>(in T[] data)
        where T : unmanaged
    {
        fixed (void* d = data)
        {
            GL.NamedBufferData(
                Handle,
                (nuint)(sizeof(T) * data.Length),
                d,
                VertexBufferObjectUsage.DynamicDraw
            );
        }
    }

    public unsafe void* MapBufferRange(int size, MapBufferAccessMask access) =>
        MapBufferRange((uint)size, access);

    public unsafe void* MapBufferRange(uint length, MapBufferAccessMask access)
    {
        //int length = (int)(Math.Round((size) / (double)ALIGNMENT) * (double)ALIGNMENT + ALIGNMENT);

        return GL.MapNamedBufferRange(Handle, 0, length, access);
    }

    public void UnmapBuffer()
    {
        GL.UnmapNamedBuffer(Handle);
    }

    public virtual void Unbind()
    {
        // FIXME cross static ref to BaseGameEngine
        GL.BindBuffer(Type, 0);
    }
}
