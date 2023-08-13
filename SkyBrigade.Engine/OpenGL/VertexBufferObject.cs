using Silk.NET.OpenGL;

namespace SkyBrigade.Engine.OpenGL;

public class VertexBufferObject<T> : IDisposable where T : unmanaged
{
    /* In OpenGL, buffers can store a wide variety of data
     * and all buffers require us to specify the data type
     * aswell as its use case; this assists us in constructing
     * more complicated buffers that require multiple smaller
     * buffers to contruct.
     */

    private BufferObject<T> vbo;
    private BufferObject<uint> ebo;
    private VertexArrayObject<T, uint> vao;

    public BufferObject<T> VertexBuffer { get => vbo; }
    public BufferObject<uint> ElementBuffer { get => ebo; }

    public VertexBufferObject(GL gl)
    {
        /* The EBO (element buffer object) stores our element array
         * ie. the order in which to draw vertices.
         */
        ebo = new(BufferTargetARB.ElementArrayBuffer);

        /* The Vertex Buffer Object stores the raw vertices data
         */
        vbo = new(BufferTargetARB.ArrayBuffer);

        /* The VBO and EBO would be useless without a way of understanding
         * the structure of the data, thats where the VAO comes in. It
         * stores the structure of the vertex data, how it is in RAM.
         */
        vao = new(gl, vbo, ebo);
    }

    /* Forwarding this method.
     */

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        vao.VertexAttributePointer(index, count, type, vertexSize, offSet);
    }

    public void Bind() => vao.Bind();

    public void Unbind() => vao.Unbind();

    /* I guess the benifit of using a managed language is that i can
     * trust the garbage collector to dispose of these. (fatal mistake)
     */

    public void Dispose()
    {
        vao.Dispose();
        ebo.Dispose();
        vbo.Dispose();
    }
}