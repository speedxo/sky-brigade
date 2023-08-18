using Silk.NET.OpenGL;
using Horizon.Data;

namespace Horizon.OpenGL;

//The vertex array object abstraction.
public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TIndexType: unmanaged where TVertexType: unmanaged
{
    //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint _handle;

    private GL _gl;
    private BufferObject<TVertexType> vbo;
    private BufferObject<TIndexType> ebo;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        //Saving the GL instance.
        _gl = gl;

        this.vbo = vbo;
        this.ebo = ebo;

        // Setting out handle and binding the VBO and EBO to this VAO.
        _handle = _gl.GenVertexArray();

        /* Binding a VBO and an EBO to a VAO in OpenGL
         * simply requires binding a VAO and then binding
         * a EBO and a VBO to it, easy peasy.
         */
        Bind();
        Unbind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        // Setting up a vertex attribute pointer
        Bind();

        _gl.VertexAttribPointer(index, count, type, false, vertexSize, (void*)(offSet));
        _gl.EnableVertexAttribArray(index);

        _gl.BindVertexArray(0);
    }

    public void Bind()
    {
        // Binding the vertex array.
        _gl.BindVertexArray(_handle);
        vbo.Bind();
        ebo.Bind();
    }

    public void Unbind()
    {
        // Unbinding the vertex array.
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public void Dispose()
    {
        // Remember to dispose this object so the data GPU side is cleared.
        // We dont delete the VBO and EBO here, as you can have one VBO stored under multiple VAO's.
        _gl.DeleteVertexArray(_handle);
    }
}