using Horizon.OpenGL;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;
using Horizon.Content;
using Shader = Horizon.Content.Shader;
using System.Diagnostics;
using Box2D.NetStandard.Common;
using Microsoft.Extensions.Options;
using System.Xml.Linq;

namespace Horizon.Rendering.Spriting.Components;

 
public class SpriteBatchMesh : Mesh2D
{
    /// <summary>
    /// This limit is set at 750 to meet the 64kb UBO limit (we should instead pack into a TBO, UPDATE: we will use instancing instead.)
    /// </summary>
    public static readonly int MAX_SPRITES = 750;

    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public Matrix4x4 modelMatrix;
        public Vector2 spriteOffset;
        public int spriteId;
        public bool isFlipped;
    }

    private readonly Spritesheet sheet;

    public UniformBufferObject UniformBuffer { get; init; }

    public bool ShouldUniformBufferUpdate { get; set; }
    public uint ElementCount { get; private set; }

    private SpriteData[] data = new SpriteData[MAX_SPRITES];

    public SpriteBatchMesh(Spritesheet sheet, Shader shader)
        :base()
    {
        this.sheet = sheet;
        this.Material = new CustomMaterial(this.sheet, in shader);
        
        UniformBuffer = new UniformBufferObject(
           GameEntity.Entity.Engine.GL.GetUniformBlockIndex(shader.Handle, "SpriteUniforms")
       );
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        throw new Exception("Please only draw a SpriteBatchMesh through a SpriteBatch");
    }

    public void Draw(
        Spritesheet sheet,
        Matrix4x4 modelMatrix,
        IEnumerable<Sprite> sprites,
        ref RenderOptions options
        )
    {
        if (ElementCount < 1)
            return; // Don't render if there is nothing to render to improve performance.

        BindAndSetUniforms(in options);

        Material.Use(in options);
        Material.Technique.SetUniform("uModel", modelMatrix);
        Material.Technique.SetUniform("uSingleFrameSize", sheet.SingleSpriteSize);

        if (ShouldUniformBufferUpdate)
        {
            ShouldUniformBufferUpdate = false;
        }

        //UniformBuffer.BufferData(new ReadOnlySpan<SpriteData>(AggregateSpriteData(sprites)));
        UniformBuffer.BufferSingleData(AggregateSpriteData(sprites));
        UniformBuffer.BindToUniformBlockBindingPoint();
        Buffer.Bind();

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

        Buffer.Unbind();

        Material.End();
    }

    public override void Load(in IMeshData<Vertex2D> data, in Material? mat = null)
    {
        base.Load(data, mat);
        ElementCount = (uint)data.Elements.Length;
    }

    protected override void BindAndSetUniforms(in RenderOptions options)
    {
        base.BindAndSetUniforms(options);
        SetUniform("uSingleFrameSize", sheet.SingleSpriteSize);
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