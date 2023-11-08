using Horizon.GameEntity;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/* This is an abstractation for a buffer object */

public class BufferObject<T> : IDisposable
    where T : unmanaged
{
    /* These are private because they have no reason to be public
     * Traditional OOP style principles break when you have to abstract
     */
    public uint Handle { get; init; }

    public readonly BufferTargetARB Type;

    public unsafe BufferObject(BufferTargetARB bufferType)
    {
        Type = bufferType;

        // FIXME cross static ref to Entity.Engine
        Handle = Entity.Engine.GL.GenBuffer();
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
