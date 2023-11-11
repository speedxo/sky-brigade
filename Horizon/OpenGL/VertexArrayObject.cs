using Horizon.GameEntity;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

//The vertex array object abstraction.
public class VertexArrayObject : IDisposable
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint _handle;

    private uint _vboHandle,
        _eboHandle;

    public VertexArrayObject(in uint vboHandle, in uint eboHandle)
    {
        this._vboHandle = vboHandle;
        this._eboHandle = eboHandle;

        // Setting out handle and binding the VBO and EBO to this VAO.
        _handle = Entity.Engine.GL.GenVertexArray();

        /* Binding a VBO and an EBO to a VAO in OpenGL
         * simply requires binding a VAO and then binding
         * a EBO and a VBO to it, easy peasy.
         */
        Bind();
        Entity.Engine.GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _eboHandle);
        Entity.Engine.GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vboHandle);
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
        Entity.Engine.GL.VertexAttribPointer(
            index,
            count,
            type,
            false,
            vertexSize,
            (void*)(offSet)
        );
        Entity.Engine.GL.EnableVertexAttribArray(index);
    }

    public void VertexAttributeDivisor(uint index, uint divisor)
    {
        // Setting up a vertex attribute pointer
        Bind();

        Entity.Engine.GL.VertexAttribDivisor(index, divisor);

        Unbind();
    }

    public void Bind()
    {
        // Binding the vertex array.
        Entity.Engine.GL.BindVertexArray(_handle);
    }

    public void Unbind()
    {
        // Unbinding the vertex array.
        Entity.Engine.GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        // Remember to dispose this object so the data GPU side is cleared.
        // We dont delete the VBO and EBO here, as you can have one VBO stored under multiple VAO's.
        Entity.Engine.GL.DeleteVertexArray(_handle);
    }
}
