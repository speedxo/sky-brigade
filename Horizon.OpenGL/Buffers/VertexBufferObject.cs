using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.OpenGL.Buffers;

//The vertex array object abstraction.
public class VertexBufferObject: IDisposable, IGLObject
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    public uint Handle { get; init; }

    public BufferObject VertexBuffer { get; init; }
    public BufferObject ElementBuffer { get; init; }


    public VertexBufferObject()
    {
        VertexBuffer = new();
        ElementBuffer = new();

        // Setting out handle and binding the VBO and EBO to this VAO.
        Handle = BaseGameEngine.GL.GenVertexArray();

        /* Binding a VBO and an EBO to a VAO in OpenGL
         * simply requires binding a VAO and then binding
         * a EBO and a VBO to it, easy peasy.
         */
        Bind();
        VertexBuffer.Bind();
        ElementBuffer.Bind();
        Unbind();
    }

    public unsafe void VertexAttributePointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        int offSet
    )
    {
        BaseGameEngine.GL.VertexAttribPointer(
            index,
            count,
            type,
            false,
            vertexSize,
            (void*)(offSet)
        );
        BaseGameEngine.GL.EnableVertexAttribArray(index);
    }

    public void VertexAttributeDivisor(uint index, uint divisor)
    {
        BaseGameEngine.GL.VertexAttribDivisor(index, divisor);
    }

    public virtual void Bind()
    {
        // Binding the vertex array.
        BaseGameEngine.GL.BindVertexArray(Handle);
    }

    public virtual void Unbind()
    {
        // Unbinding the vertex array.
        BaseGameEngine.GL.BindVertexArray(0);
    }

    public virtual void Dispose()
    {
        // Remember to dispose this object so the data GPU side is cleared.
        //// IGNORE FOR NOW: We dont delete the VBO and EBO here, as you can have one VBO stored under multiple VAO's.
        BaseGameEngine.GL.DeleteVertexArray(Handle);
        VertexBuffer.Dispose();
        ElementBuffer.Dispose();

        GC.SuppressFinalize(this);
    }
}
