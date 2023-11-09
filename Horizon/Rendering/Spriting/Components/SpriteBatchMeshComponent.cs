using Horizon.OpenGL;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    private BufferStorageObject<SpriteData> storageBuffer { get; init; }
    private unsafe SpriteData* dataPtr;

    public uint ElementCount { get; private set; }
    private uint bufferLength = 16;

    public SpriteBatchMesh(SpriteSheet sheet, Shader shader)
        : base()
    {
        this.sheet = sheet;
        this.Material = new CustomMaterial(this.sheet, in shader);

        storageBuffer = new(
            BufferStorageTarget.ShaderStorageBuffer,
            BufferTargetARB.ShaderStorageBuffer
        );

        unsafe
        {
            dataPtr = (SpriteData*)
                storageBuffer.MapBufferRange(
                    (nuint)(bufferLength * sizeof(SpriteData)),
                    MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit
                );
        }

        Material.Technique.Shader.GetResourceIndex(
            "SpriteUniforms",
            ProgramInterface.ShaderStorageBlock
        );
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        throw new Exception("Please only draw a SpriteBatchMesh through a SpriteBatch");
    }

    public unsafe void Draw(
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

        // I AM TESING STUFF!!!!

        if (sprites.Length > bufferLength) // Check if array has been resized.
        {
            bufferLength = (uint)sprites.Length;

            storageBuffer.UnmapBuffer();
            Console.WriteLine("REMAP");
            dataPtr = (SpriteData*)
                storageBuffer.MapBufferRange(
                    (nuint)(bufferLength * sizeof(SpriteData)),
                    MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit
                );
        }

        AggregateSpriteData(in sprites);

        Material.Technique.Shader.BindBuffer("SpriteUniforms", storageBuffer);
        Buffer.VertexArray.Bind();

        // Turn on wire-frame mode
        if (options.IsWireframeEnabled)
            GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        GameEntity.Entity.Engine.GL.DrawElements(
            PrimitiveType.Triangles,
            ElementCount,
            DrawElementsType.UnsignedInt,
            null
        );

        // Turn off wire-frame mode
        if (options.IsWireframeEnabled)
            GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

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

    private unsafe void AggregateSpriteData(in ReadOnlySpan<Sprite> sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            dataPtr[i].modelMatrix = sprites[i].Transform.ModelMatrix;
            dataPtr[i].spriteOffset = sprites[i].GetFrameOffset();
        }
    }

    ~SpriteBatchMesh()
    {
        storageBuffer.UnmapBuffer();
    }
}
