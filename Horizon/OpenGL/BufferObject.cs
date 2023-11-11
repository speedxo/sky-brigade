using HidSharp.Reports;
using Horizon.GameEntity;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/* This is an abstraction for a buffer object */

public class BufferObject<T> : IDisposable
    where T : unmanaged
{
    /* These are private because they have no reason to be public
     * Traditional OOP style principles break when you have to abstract
     */
    public uint Handle { get; init; }

    public readonly BufferTargetARB Type;

    internal static long ALIGNMENT = 0;

    static BufferObject()
    {
        ALIGNMENT = Entity.Engine.GL.GetInteger64(GetPName.MinMapBufferAlignment);
    }

    public unsafe BufferObject(BufferTargetARB bufferType)
    {
        Type = bufferType;

        // FIXME cross static ref to Entity.Engine
        Handle = Entity.Engine.GL.CreateBuffer();
    }

    public unsafe void BufferStorage(
        uint size,
        BufferStorageMask masks =
            BufferStorageMask.MapPersistentBit
            | BufferStorageMask.MapCoherentBit
            | BufferStorageMask.MapWriteBit
    )
    {
        Entity.Engine.GL.NamedBufferStorage(Handle, (nuint)(size), null, masks);
    }

    public virtual unsafe void BufferData(in ReadOnlySpan<T> data)
    {
        Bind();

        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BufferData(
            Type,
            (nuint)(data.Length * sizeof(T)),
            data,
            BufferUsageARB.DynamicDraw
        );

        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferSubData(in ReadOnlySpan<T> data, int offset = 0)
    {
        Bind();

        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BufferSubData(Type, offset, (nuint)(sizeof(T) * data.Length), data);

        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferSubData(in T[] data, int offset = 0)
    {
        Bind();

        fixed (void* d = data)
        {
            // FIXME cross static ref to Entity.Engine
            Entity.Engine.GL.BufferSubData(Type, offset, (nuint)(sizeof(T) * data.Length), d);
        }

        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, 0);
    }

    public virtual unsafe void BufferData(in T[] data)
    {
        Bind();

        fixed (void* d = data)
        {
            // FIXME cross static ref to Entity.Engine
            Entity.Engine.GL.BufferData(
                Type,
                (nuint)(data.Length * sizeof(T)),
                d,
                BufferUsageARB.DynamicDraw
            );
        }
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, 0);
    }

    public virtual void Bind()
    {
        /* Binding the buffer object, with the correct buffer type.
         */
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, Handle);
    }

    public virtual unsafe void NamedBufferData(in ReadOnlySpan<T> data)
    {
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.NamedBufferData(
            Handle,
            (nuint)(data.Length * sizeof(T)),
            data,
            VertexBufferObjectUsage.DynamicDraw
        );
    }

    public virtual unsafe void NamedBufferSubData(in ReadOnlySpan<T> data, int offset = 0)
    {
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.NamedBufferSubData(Handle, offset, (nuint)(sizeof(T) * data.Length), data);
    }

    public virtual unsafe void NamedBufferSubData(in T[] data, int offset = 0)
    {
        fixed (void* d = data)
        {
            Entity.Engine.GL.NamedBufferData(
                Handle,
                (nuint)(sizeof(T) * data.Length),
                d,
                VertexBufferObjectUsage.DynamicDraw
            );
        }
    }

    public virtual unsafe void NamedBufferData(in T[] data)
    {
        fixed (void* d = data)
        {
            Entity.Engine.GL.NamedBufferData(
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

        return Entity.Engine.GL.MapNamedBufferRange(Handle, 0, (nuint)length, access);
    }

    public void UnmapBuffer()
    {
        Entity.Engine.GL.UnmapNamedBuffer(Handle);
    }

    public virtual void Unbind()
    {
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(Type, 0);
    }

    public virtual void Dispose()
    {
        try
        {
            // FIXME cross static ref to Entity.Engine
            Entity.Engine.GL.DeleteBuffer(Handle);
        }
        catch
        {
            /* i dont fucking care */
        }
        GC.SuppressFinalize(this);
    }
}
