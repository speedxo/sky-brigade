using Horizon.OpenGL;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shader = Horizon.Content.Shader;

namespace Horizon.Rendering.Spriting.Components;

public class SpriteBatchMesh : Mesh2D
{
    private const string UNIFORM_SINGLE_BUFFER_SIZE = "uSingleFrameSize";

    // TODO: we'll get back to memory alignment later.
    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public Matrix4x4 modelMatrix;
        public Vector2 spriteOffset;
        public uint frameIndex;
        private uint spacer6;
    }

    private readonly SpriteSheet sheet;
    private BufferObject<SpriteData> storageBuffer { get; init; }
    private unsafe SpriteData* dataPtr;

    public uint ElementCount { get; private set; }
    private uint bufferLength = 0;

    public unsafe SpriteBatchMesh(SpriteSheet sheet, Shader shader)
        : base()
    {
        unsafe
        {
            Console.WriteLine($"sizeof(SpriteData) = {sizeof(SpriteData)}");
            Console.WriteLine($"BufferObject Alignment = {BufferObject<SpriteData>.ALIGNMENT}");
            Debug.Assert(sizeof(SpriteData) % 16 == 0);
            bufferLength = (uint)(BufferObject<SpriteData>.ALIGNMENT * 128);
        }

        this.sheet = sheet;
        this.Material = new CustomMaterial(this.sheet, in shader);

        storageBuffer = new(BufferTargetARB.ShaderStorageBuffer);
        storageBuffer.BufferStorage((uint)sizeof(SpriteData) * 65565);

        GenerateMesh();

        unsafe
        {
            dataPtr = (SpriteData*)
                storageBuffer.MapBufferRange(
                    (uint)(bufferLength),
                    MapBufferAccessMask.WriteBit
                        | MapBufferAccessMask.PersistentBit
                        | MapBufferAccessMask.CoherentBit
                );
        }

        Material.Technique.Shader.GetResourceIndex(
            "spriteData",
            ProgramInterface.ShaderStorageBlock
        );
    }

    private void GenerateMesh()
    {
        float size = 1.0f;

        Vertex2D[] vertices = new Vertex2D[]
        {
            new Vertex2D(-size / 2.0f, -size / 2.0f, 0, 1),
            new Vertex2D(size / 2.0f, -size / 2.0f, 1, 1),
            new Vertex2D(size / 2.0f, size / 2.0f, 1, 0),
            new Vertex2D(-size / 2.0f, size / 2.0f, 0, 0),
        };
        uint[] elements = new uint[] { 0, 1, 2, 0, 2, 3 };

        Buffer.VertexBuffer.BufferData(vertices);
        Buffer.ElementBuffer.BufferData(elements);
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        throw new Exception("Please only draw a SpriteBatchMesh through a SpriteBatch");
    }

    public unsafe void Draw(in ReadOnlySpan<Sprite> sprites, ref RenderOptions options)
    {
        BindAndSetUniforms(in options);
        Material.Use(in options);

        // I AM TESING STUFF!!!!

        if (sprites.Length > bufferLength) // Check if array has been resized.
        {
            bufferLength = (uint)sprites.Length;

            storageBuffer.UnmapBuffer();
            dataPtr = (SpriteData*)
                storageBuffer.MapBufferRange(
                    (uint)(bufferLength * sizeof(SpriteData)),
                    MapBufferAccessMask.WriteBit
                        | MapBufferAccessMask.PersistentBit
                        | MapBufferAccessMask.CoherentBit
                );
        }

        AggregateSpriteData(in sprites);

        Material.Technique.Shader.BindBuffer("spriteData", storageBuffer);
        Buffer.VertexArray.Bind();

        // Turn on wire-frame mode
        if (options.IsWireframeEnabled)
            GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        GameEntity.Entity.Engine.GL.DrawElementsInstanced(
            PrimitiveType.Triangles,
            6,
            DrawElementsType.UnsignedInt,
            null,
            (uint)sprites.Length
        );

        // Turn off wire-frame mode
        if (options.IsWireframeEnabled)
            GameEntity.Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        Buffer.VertexArray.Unbind();

        Material.End();
    }

    public override void Load(in IMeshData<Vertex2D> data, in Material? mat = null)
    {
        //base.Load(data, mat);
        //ElementCount = (uint)data.Elements.Length;
    }

    protected override void BindAndSetUniforms(in RenderOptions options)
    {
        base.BindAndSetUniforms(options);
        SetUniform(UNIFORM_SINGLE_BUFFER_SIZE, sheet.SingleSpriteSize);
    }

    private unsafe void AggregateSpriteData(in ReadOnlySpan<Sprite> sprites)
    {
        if (dataPtr == null)
            return;
        for (int i = 0; i < sprites.Length; i++)
        {
            dataPtr[i].modelMatrix = sprites[i].Transform.ModelMatrix;
            dataPtr[i].spriteOffset = sprites[i].GetFrameOffset();
            dataPtr[i].frameIndex = sprites[i].GetFrameIndex();
        }
    }

    ~SpriteBatchMesh()
    {
        storageBuffer.UnmapBuffer();
    }
}
