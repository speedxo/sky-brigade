using Horizon.OpenGL;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;
using Shader = Horizon.Content.Shader;

namespace Horizon.Rendering.Spriting.Components;

public class SpriteBatchMesh : Mesh2D
{
    private const string UNIFORM_SINGLE_BUFFER_SIZE = "uSingleFrameSize";

    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public Matrix4x4 modelMatrix;
        public Vector2 spriteOffset;
    }

    private readonly SpriteSheet sheet;

    private BufferObject<SpriteData> storageBuffer { get; init; }

    public uint ElementCount { get; private set; }
    private uint bufferLength = 0;

    private SpriteData[] data = new SpriteData[16];

    public SpriteBatchMesh(SpriteSheet sheet, Shader shader)
        : base()
    {
        this.sheet = sheet;
        this.Material = new CustomMaterial(this.sheet, in shader);

        storageBuffer = new(BufferTargetARB.ShaderStorageBuffer);
        Material.Technique.Shader.GetResourceIndex(
            "SpriteUniforms",
            ProgramInterface.ShaderStorageBlock
        );
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        throw new Exception("Please only draw a SpriteBatchMesh through a SpriteBatch");
    }

    public void Draw(
        in SpriteSheet sheet,
        in Matrix4x4 modelMatrix,
        in ReadOnlySpan<Sprite> sprites,
        ref RenderOptions options
    )
    {
        if (ElementCount < 1)
            return; // Don't render if there is nothing to render to improve performance.

        BindAndSetUniforms(in options);

        Material.Use(in options);
        Material.Technique.SetUniform(UNIFORM_MODEL_MATRIX, modelMatrix);
        Material.Technique.SetUniform(UNIFORM_SINGLE_BUFFER_SIZE, sheet.SingleSpriteSize);

        //storageBuffer.BufferData(new ReadOnlySpan<SpriteData>(AggregateSpriteData(sprites)));

        // I AM TESING STUFF!!!!

        AggregateSpriteData(in sprites);
        if (data.Length > bufferLength)
        {
            storageBuffer.BufferData(new ReadOnlySpan<SpriteData>(data));
            bufferLength = (uint)data.Length;
        }
        else
            storageBuffer.BufferSubData(new ReadOnlySpan<SpriteData>(data));

        Material.Technique.Shader.BindBuffer("SpriteUniforms", storageBuffer);
        Buffer.VertexArray.Bind();

        // Once again, I really don't want to make the whole method unsafe for one call.
        unsafe
        {
            // Turn on wire-frame mode
            if (options.IsWireframeEnabled)
                GameEntity.Entity.Engine.GL.PolygonMode(
                    TriangleFace.FrontAndBack,
                    PolygonMode.Line
                );

            GameEntity.Entity.Engine.GL.DrawElements(
                PrimitiveType.Triangles,
                ElementCount,
                DrawElementsType.UnsignedInt,
                null
            );

            // Turn off wire-frame mode
            if (options.IsWireframeEnabled)
                GameEntity.Entity.Engine.GL.PolygonMode(
                    TriangleFace.FrontAndBack,
                    PolygonMode.Fill
                );
        }

        Buffer.VertexArray.Unbind();

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
        SetUniform(UNIFORM_SINGLE_BUFFER_SIZE, sheet.SingleSpriteSize);
    }

    private void AggregateSpriteData(in ReadOnlySpan<Sprite> sprites)
    {
        if (sprites.Length > data.Length)
            Array.Resize(ref data, sprites.Length);

        for (int i = 0; i < sprites.Length; i++)
        {
            data[i].modelMatrix = sprites[i].Transform.ModelMatrix;
            data[i].spriteOffset = sprites[i].GetFrameOffset();
        }
    }
}
