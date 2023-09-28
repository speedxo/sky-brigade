using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Horizon.Rendering.Spriting.Components;

public class SpriteBatchMesh
{
    public static readonly int MAX_SPRITES = 750;

    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public Matrix4x4 modelMatrix;
        public Vector2 spriteOffset;
        public int spriteId;
        public bool isFlipped;
    }

    private SpriteData[] data = new SpriteData[MAX_SPRITES];

    public ShaderComponent Shader { get; init; }
    public UniformBufferObject UniformBuffer { get; init; }

    public bool ShouldUniformBufferUpdate { get; set; }

    public uint ElementCount { get; private set; }
    public VertexBufferObject<Vertex2D> Vbo { get; private set; }

    public SpriteBatchMesh(ShaderComponent shader)
    {
        this.Shader = shader;
        UniformBuffer = new UniformBufferObject(
            GameEntity.Entity.Engine.GL.GetUniformBlockIndex(shader.Handle, "SpriteUniforms")
        );

        unsafe
        {
            Console.WriteLine(sizeof(SpriteData) % 16 == 0);
        }
        Vbo = new();

        Vbo.VertexAttributePointer(
            0,
            2,
            VertexAttribPointerType.Float,
            (uint)Vertex2D.SizeInBytes,
            0
        );
        Vbo.VertexAttributePointer(
            1,
            2,
            VertexAttribPointerType.Float,
            (uint)Vertex2D.SizeInBytes,
            2 * sizeof(float)
        );
        Vbo.VertexAttributePointer(
            2,
            1,
            VertexAttribPointerType.Int,
            (uint)Vertex2D.SizeInBytes,
            4 * sizeof(float)
        );
    }

    public void Upload(ReadOnlySpan<Vertex2D> vertices, ReadOnlySpan<uint> elements)
    {
        Vbo.VertexBuffer.BufferData(vertices);
        Vbo.ElementBuffer.BufferData(elements);

        ElementCount = (uint)elements.Length;
    }

    public void Draw(
        Spritesheet sheet,
        Matrix4x4 modelMatrix,
        IEnumerable<Sprite> sprites,
        RenderOptions? renderOptions = null
    )
    {
        if (ElementCount < 1)
            return; // Don't render if there is nothing to render to improve performance.

        var options = renderOptions ?? RenderOptions.Default;

        Shader.Use();

        GameEntity.Entity.Engine.GL.ActiveTexture(TextureUnit.Texture0);
        GameEntity.Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, sheet.Texture.Handle);
        Shader.SetUniform("uTexture", 0);

        if (ShouldUniformBufferUpdate)
        {
            ShouldUniformBufferUpdate = false;
        }

        //UniformBuffer.BufferData(new ReadOnlySpan<SpriteData>(AggregateSpriteData(sprites)));
        UniformBuffer.BufferSingleData(AggregateSpriteData(sprites));
        UniformBuffer.BindToUniformBlockBindingPoint();

        Shader.SetUniform("uView", options.Camera.View);
        Shader.SetUniform("uProjection", options.Camera.Projection);
        Shader.SetUniform("uModel", modelMatrix);
        Shader.SetUniform("uSingleFrameSize", sheet.SingleSpriteSize);
        Shader.SetUniform("uWireframeEnabled", options.IsWireframeEnabled ? 1 : 0);

        Vbo.Bind();

        // Once again, I really don't want to make the whole method unsafe for one call.
        unsafe
        {
            // Turn on wireframe mode
            if (options.IsWireframeEnabled)
                GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

            GameEntity.Entity.Engine.GL.DrawElements(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null
            );

            // Turn off wireframe mode
            if (options.IsWireframeEnabled)
                GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        }

        Vbo.Unbind();

        Shader.End();
    }

    private SpriteData[] AggregateSpriteData(IEnumerable<Sprite> sprites)
    {
        int i = 0;
        foreach (var sprite in sprites)
        {
            data[i] = new SpriteData
            {
                modelMatrix = sprite.Transform.ModelMatrix,
                spriteOffset = sprite.GetFrameOffset(),
                spriteId = sprite.ID,
                isFlipped = sprite.Flipped
            };
            i++;
        }
        return data;
    }
}