using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL;

/* This is an abstractation for a buffer object */

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    /* These are private because they have no reason to be public
     * Traditional OOP style principles break when you have to abstract
     */
    private uint _handle;
    private BufferTargetARB _bufferType;
    private GL _gl;

    public unsafe BufferObject(GL gl, BufferTargetARB bufferType)
    {
        /* This OpenGL translation library unfortunatly
         * requires us to copy around this loose reference to the GL class
         */
        _gl = gl;
        _bufferType = bufferType;

        _handle = _gl.GenBuffer();
    }

    public unsafe void BufferData(ReadOnlySpan<TDataType> data)
    {
        Bind();
        fixed (void* d = data)
        {
            _gl.BufferData(_bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
        _gl.BindBuffer(_bufferType, 0);
    }

    public void Bind()
    {
        /* Binding the buffer object, with the correct buffer type.
         */
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        try
        {
            _gl.DeleteBuffer(_handle);
        }
        catch (Exception ex)
        {
        }
    }
}