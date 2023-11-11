using Horizon.Data;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Silk.NET.OpenGL;

namespace Horizon.Rendering;

/// <summary>
/// A new more integrated abstraction for handling geometry.
/// </summary>
public class Mesh3D : Mesh<Vertex>
{
    public VertexBufferObject<Vertex> Buffer { get; init; }

    private uint ElementCount = 0;

    public Mesh3D()
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

        Buffer.VertexAttributePointer(
            0,
            3,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            0
        );
        Buffer.VertexAttributePointer(
            1,
            3,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            3 * sizeof(float)
        );
        Buffer.VertexAttributePointer(
            2,
            2,
            VertexAttribPointerType.Float,
            (uint)Vertex.SizeInBytes,
            6 * sizeof(float)
        );

        Buffer.VertexBuffer.Unbind();
        Buffer.VertexArray.Unbind();
    }

    public override void Render(float dt, ref RenderOptions options)
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

    protected virtual void BindAndSetUniforms(in RenderOptions options)
    {
        Material.Use(in options);

        SetUniform(UNIFORM_VIEW_MATRIX, options.Camera.View);
        SetUniform(UNIFORM_PROJECTION_MATRIX, options.Camera.Projection);
        SetUniform(UNIFORM_USE_WIREFRAME, options.IsWireframeEnabled ? 1 : 0);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);

        Buffer.Dispose();
    }

    /// <summary>
    /// Loads mesh data into the mesh, please note that material is only set if it is null.
    /// </summary>
    public override void Load(in IMeshData<Vertex> data, in Material? mat = null)
    {
        Buffer.VertexBuffer.BufferData(data.Vertices.Span);
        Buffer.ElementBuffer.BufferData(data.Elements.Span);

        ElementCount = (uint)data.Elements.Length;
        Material ??= mat ?? new EmptyMaterial();
    }
}
