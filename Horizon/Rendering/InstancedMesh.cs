using Horizon.GameEntity;
using Horizon.OpenGL;
using Silk.NET.OpenGL;

namespace Horizon.Rendering;

/// <summary>
/// A new more integrated abstraction for handling geometry.
/// </summary>
public abstract class InstancedMesh<VertexType, InstancedDataType> : Mesh<VertexType>
    where InstancedDataType : unmanaged
    where VertexType : unmanaged
{
    public InstancedVertexBufferObject<VertexType, InstancedDataType> Buffer { get; init; }

    public uint Count { get; set; }

    private uint ElementCount = 0;

    public InstancedMesh()
    {
        Buffer = new();
        SetVboLayout();
    }

    /// <summary>
    /// Telling the VAO object how to lay out the attributes.
    /// </summary>
    protected abstract void SetVboLayout();

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

            Entity.Engine.GL.DrawElementsInstanced(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null,
                Count
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

    public override void Load(in IMeshData<VertexType> data, in Material? mat = null)
    {
        Buffer.VertexBuffer.BufferData(data.Vertices.Span);
        Buffer.ElementBuffer.BufferData(data.Elements.Span);

        ElementCount = (uint)data.Elements.Length;
        Material = mat ?? new EmptyMaterial();
    }

    public abstract void Load(
        in InstancedMeshData<VertexType, InstancedDataType> data,
        in Material? mat = null
    );
}
