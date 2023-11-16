using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;

namespace Horizon.Rendering;

public class RenderRectangle : GameObject
{
    private static VertexBufferObject vbo;

    public Technique Technique { get; init; }

    public RenderRectangle(in Technique technique)
    {
        this.Technique = technique;
    }

    public override void Initialize()
    {
        base.Initialize();
        if (vbo is null)
        {
            var verts = new Vector2[]
            {
                new Vector2(-1, -1),
                new Vector2(0, 1),
                new Vector2(1, -1),
                new Vector2(1, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(-1, 1),
                new Vector2(0, 0)
            };
            var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

            vbo = new VertexBufferObject(
                Engine.Content.VertexArrays.Create(VertexArrayObjectDescription.VertexBuffer)
            );

            vbo.Bind();
            {
                // setup vertex buffer
                vbo.VertexBuffer.Bind();
                vbo.VertexAttributePointer(
                    0,
                    2,
                    Silk.NET.OpenGL.VertexAttribPointerType.Float,
                    sizeof(float) * 4,
                    0
                );
                vbo.VertexAttributePointer(
                    1,
                    2,
                    Silk.NET.OpenGL.VertexAttribPointerType.Float,
                    sizeof(float) * 4,
                    sizeof(float) * 2
                );

                // setup element buffer
                vbo.ElementBuffer.Bind();
            }
            vbo.Unbind();
            vbo.VertexBuffer.Unbind();
            vbo.ElementBuffer.Unbind();

            vbo.VertexBuffer.NamedBufferData(verts);
            vbo.ElementBuffer.NamedBufferData(indices);
        }
    }

    public override unsafe void Render(float dt)
    {
        base.Render(dt);
        Technique.Bind();

        vbo.Bind();
        Engine
            .GL
            .DrawElements(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                6,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                null
            );
        vbo.Unbind();
        Technique.Unbind();
    }
}
