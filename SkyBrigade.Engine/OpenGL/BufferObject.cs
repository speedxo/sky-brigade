using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL;

/* This is an abstractation for a buffer object */

public class BufferObject : IDisposable
{
    /* These are private because they have no reason to be public
     * Traditional OOP style principles break when you have to abstract
     */
    public uint Handle { get; init; }

    private readonly BufferTargetARB _bufferType;

    public unsafe BufferObject(BufferTargetARB bufferType)
    {
        _bufferType = bufferType;

        Handle = GameManager.Instance.Gl.GenBuffer();
    }

    public virtual unsafe void BufferData<TDataType>(ReadOnlySpan<TDataType> data)
        where TDataType : unmanaged
    {
        Bind();
        fixed (void* d = data)
        {
            GameManager.Instance.Gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
        GameManager.Instance.Gl.BindBuffer(_bufferType, 0);
    }

    public virtual void Bind()
    {
        /* Binding the buffer object, with the correct buffer type.
         */
        GameManager.Instance.Gl.BindBuffer(_bufferType, Handle);
    }

    public virtual void Dispose()
    {
        try
        {
            GameManager.Instance.Gl.DeleteBuffer(Handle);
        }
        catch
        {
            /* i dont fucking care */
        }
        GC.SuppressFinalize(this);
    }
}