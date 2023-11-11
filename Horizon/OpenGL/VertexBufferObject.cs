using Silk.NET.OpenGL;

namespace Horizon.OpenGL;

/// <summary>
/// An abstraction around a simple vbo providing an element buffer, a vertex buffer and a vertex array object.
/// </summary>
/// <typeparam name="T">The vertex type.</typeparam>
public class VertexBufferObject<T> : IDisposable
    where T : unmanaged
{
    /* In OpenGL, buffers can store a wide variety of data
     * and all buffers require us to specify the data type
     * aswell as its use case; this assists us in constructing
     * more complicated buffers that require multiple smaller
     * buffers to contruct.
     */

    public BufferObject<T> VertexBuffer { get; init; }
    public BufferObject<uint> ElementBuffer { get; init; }
    public VertexArrayObject VertexArray { get; init; }

    public VertexBufferObject()
    {
        /* The EBO (element buffer object) stores our element array
         * ie. the order in which to draw vertices.
         */
        ElementBuffer = new(BufferTargetARB.ElementArrayBuffer);

        /* The Vertex Buffer Object stores the raw vertices data
         */
        VertexBuffer = new(BufferTargetARB.ArrayBuffer);

        /* The VBO and EBO would be useless without a way of understanding
         * the structure of the data, thats where the VAO comes in. It
         * stores the structure of the vertex data, how it is in RAM.
         */
        // FIXME static cross ref to Entity.Engine
        VertexArray = new(VertexBuffer.Handle, ElementBuffer.Handle);
    }

    /* Forwarding this method.
     */

    public unsafe void VertexAttributePointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        int offSet
    )
    {
        VertexArray.VertexAttributePointer(index, count, type, vertexSize, offSet);
    }

    public void VertexAttributeDivisor(uint index, uint divisor) =>
        VertexArray.VertexAttributeDivisor(index, divisor);

    public virtual void Bind()
    {
        VertexArray.Bind();
        VertexBuffer.Bind();
        ElementBuffer.Bind();
    }

    public virtual void Unbind()
    {
        VertexArray.Unbind();
        VertexBuffer.Unbind();
        ElementBuffer.Unbind();
    }

    /* I guess the benifit of using a managed language is that i can
     * trust the garbage collector to dispose of these. (fatal mistake)
     */

    public virtual void Dispose()
    {
        VertexArray.Dispose();
        ElementBuffer.Dispose();
        VertexBuffer.Dispose();
    }
}
