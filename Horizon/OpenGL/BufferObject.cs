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

    private readonly BufferTargetARB _bufferType;

    public unsafe BufferObject(BufferTargetARB bufferType)
    {
        _bufferType = bufferType;

        // FIXME cross static ref to Entity.Engine
        Handle = Entity.Engine.GL.GenBuffer();
    }

    public virtual unsafe void BufferData(ReadOnlySpan<T> data)
    {
        Bind();
        fixed (void* d = data)
        {
            // FIXME cross static ref to Entity.Engine
            Entity.Engine.GL.BufferData(
                _bufferType,
                (nuint)(data.Length * sizeof(T)),
                d,
                BufferUsageARB.StaticDraw
            );
        }
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(_bufferType, 0);
    }

    public virtual void Bind()
    {
        /* Binding the buffer object, with the correct buffer type.
         */
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(_bufferType, Handle);
    }

    public virtual void Unbind()
    {
        // FIXME cross static ref to Entity.Engine
        Entity.Engine.GL.BindBuffer(_bufferType, 0);
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
