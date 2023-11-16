using Horizon.Core;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Rendering.Primitives;


/// <summary>
/// A new more integrated abstraction for handling geometry.
/// </summary>
public class Mesh2D : GameObject
{
    private const string UNIFORM_VIEW_MATRIX = "uView";
    private const string UNIFORM_PROJECTION_MATRIX = "uProjection";
    private const string UNIFORM_ENABLE_WIREFRAME = "uWireframeEnabled";

    public VertexBufferObject Buffer { get; private set; }

    public Technique Shader { get; protected set; }

    private uint ElementCount = 0;

    public Mesh2D(in Technique? shader=null)
    {
        Shader = shader;
    }

    public override void Initialize()
    {
        base.Initialize();

        Buffer = new VertexBufferObject(Engine.Content.VertexArrays.Create(VertexArrayObjectDescription.VertexBuffer).Asset);
        SetVboLayout();
    }

    /// <summary>
    /// Telling the VAO object how to lay out the attributes.
    /// </summary>
    protected virtual void SetVboLayout()
    {
        Buffer.Bind();
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
        Buffer.Unbind();
    }

    /// <summary>
    ///   <para>
    /// Draws the current object using the provided render options.
    /// </para>
    /// </summary>
    /// <param name="dt">The elapsed time since the last render call.</param>
    /// <param name="options">Optional render options. If not provided, default options will be used.</param>
    public override void Render(float dt)
    {
        if (ElementCount < 1)
            return; // SAVOUR THE FRAMES!!!

        Shader.Bind();
        Buffer.Bind();
        unsafe
        {
            // Once again, I really don't want to make the whole method unsafe for one call.
            Engine.GL.DrawElements(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null
            );
        }
        Buffer.Unbind();
        Shader.Unbind();
    }
}
