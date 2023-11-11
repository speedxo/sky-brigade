using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Horizon.Rendering;

/// <summary>
/// A new more integrated abstraction for handling geometry.
/// </summary>
public class Mesh2D : Mesh<Vertex2D>
{
    private const string UNIFORM_VIEW_MATRIX = "uView";
    private const string UNIFORM_PROJECTION_MATRIX = "uProjection";
    private const string UNIFORM_ENABLE_WIREFRAME = "uWireframeEnabled";

    /// <summary>The underlying VBO this class encapsulates.</summary>
    /// <value>The buffer.</value>
    public VertexBufferObject<Vertex2D> Buffer { get; init; }

    private uint ElementCount = 0;

    public Mesh2D()
    {
        Buffer = new();
        SetVboLayout();
    }

    /// <summary>
    /// Telling the VAO object how to lay out the attributes.
    /// </summary>
    protected virtual void SetVboLayout()
    {
        Buffer.VertexArray.Bind();
        Buffer.VertexBuffer.Bind();

        Buffer.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, Vertex2D.SizeInBytes, 0);
        Buffer.VertexAttributePointer(
            1,
            2,
            VertexAttribPointerType.Float,
            Vertex2D.SizeInBytes,
            2 * sizeof(float)
        );
        Buffer.VertexBuffer.Unbind();
        Buffer.VertexArray.Unbind();
    }

    /// <summary>
    ///   <para>
    /// Draws the current object using the provided render options.
    /// </para>
    /// </summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="options">Optional render options. If not provided, default options will be used.</param>
    public override void Draw(float dt, ref RenderOptions options)
    {
        if (ElementCount < 1)
            return; // SAVOUR THE FRAMES!!!

        BindAndSetUniforms(in options);

        Buffer.Bind();

        // Once again, I really don't want to make the whole method unsafe for one call.
        unsafe
        {
            // Turn on wireframe mode
            if (options.IsWireframeEnabled)
                Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            Entity.Engine.GL.DrawElements(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null
            );

            // Turn off wireframe mode
            if (options.IsWireframeEnabled)
                Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        Buffer.Unbind();
    }

    /// <summary>Binds and set shader uniforms.</summary>
    /// <param name="options">The options.</param>
    protected virtual void BindAndSetUniforms(in RenderOptions options)
    {
        Material.Use(in options);

        SetUniform(UNIFORM_VIEW_MATRIX, options.Camera.View);
        SetUniform(UNIFORM_PROJECTION_MATRIX, options.Camera.Projection);
        SetUniform(UNIFORM_ENABLE_WIREFRAME, options.IsWireframeEnabled ? 1 : 0);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);

        Buffer.Dispose();
    }

    /// <summary>
    /// Loads mesh data into the mesh, please note that material is only set if it is null.
    /// </summary>
    public override void Load(in IMeshData<Vertex2D> data, in Material? mat = null)
    {
        Buffer.VertexBuffer.BufferData(data.Vertices.Span);
        Buffer.ElementBuffer.BufferData(data.Elements.Span);

        ElementCount = (uint)data.Elements.Length;
        Material ??= mat ?? new EmptyMaterial();
    }
}
