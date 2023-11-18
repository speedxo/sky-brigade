using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Primitives;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;

namespace Horizon.Rendering.Spriting.Components;

public class SpriteBatchMesh : GameObject
{
    private const string UNIFORM_SINGLE_BUFFER_SIZE = "uSingleFrameSize";
    private const string UNIFORM_CAMERA_PROJ_MATRIX = "uCameraProjection";
    private const string UNIFORM_CAMERA_VIEW_MATRIX = "uCameraView";

    // TODO: we'll get back to memory alignment later.
    [StructLayout(LayoutKind.Sequential)]
    private struct SpriteData
    {
        public Matrix4x4 modelMatrix;
        public Vector2 spriteOffset;
        public uint frameIndex;
        private uint mermoryalignmentblahblah;
    }

    private readonly SpriteSheet sheet;

    public Technique Shader { get; init; }
    public VertexBufferObject Buffer { get; init; }
    public BufferObject StorageBuffer { get; init; }

    // eish
    private unsafe SpriteData* dataPtr;

    public uint ElementCount { get; private set; }
    private uint bufferLength = 0;

    public unsafe SpriteBatchMesh(SpriteSheet sheet, Technique shader)
        : base()
    {
        this.Shader = shader;

        bufferLength = (uint)(BufferObject.ALIGNMENT * 128);
        this.sheet = sheet;

        Buffer = new VertexBufferObject(
            Engine.ContentManager.VertexArrays.Create(VertexArrayObjectDescription.VertexBuffer)
        );

        StorageBuffer = Engine
            .ContentManager
            .Buffers
            .Create(
                new BufferObjectDescription
                {
                    IsStorageBuffer = true,
                    Size = (uint)(sizeof(SpriteData) * 65565),
                    StorageMasks =
                        BufferStorageMask.MapCoherentBit
                        | BufferStorageMask.MapPersistentBit
                        | BufferStorageMask.MapWriteBit,
                    Type = BufferTargetARB.ShaderStorageBuffer
                }
            )
            .Asset;

        SetVboLayout();

        GenerateMesh();

        unsafe
        {
            dataPtr = (SpriteData*)
                StorageBuffer.MapBufferRange(
                    (uint)(bufferLength),
                    MapBufferAccessMask.WriteBit
                        | MapBufferAccessMask.PersistentBit
                        | MapBufferAccessMask.CoherentBit
                );
        }
    }

    private void SetVboLayout()
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

    public override void Render(float dt, object? obj = null)
    {
        throw new Exception("Please only draw a SpriteBatchMesh through a SpriteBatch");
    }

    public unsafe void Draw(in ReadOnlySpan<Sprite> sprites)
    {
        BindAndSetUniforms();

        // I AM TESING STUFF!!!!

        if (sprites.Length > bufferLength) // Check if array has been resized.
        {
            bufferLength = (uint)sprites.Length;

            StorageBuffer.UnmapBuffer();
            dataPtr = (SpriteData*)
                StorageBuffer.MapBufferRange(
                    (uint)(bufferLength * sizeof(SpriteData)),
                    MapBufferAccessMask.WriteBit
                        | MapBufferAccessMask.PersistentBit
                        | MapBufferAccessMask.CoherentBit
                );
        }

        AggregateSpriteData(in sprites);

        Shader.BindBuffer("spriteData", StorageBuffer);
        Buffer.Bind();
        Buffer.VertexBuffer.Bind();
        Buffer.ElementBuffer.Bind();

        Engine
            .GL
            .DrawElementsInstanced(
                PrimitiveType.Triangles,
                6,
                DrawElementsType.UnsignedInt,
                null,
                (uint)sprites.Length
            );

        Buffer.Unbind();
        Shader.Unbind();
    }

    protected void BindAndSetUniforms()
    {
        Shader.Bind();

        Shader.SetUniform(UNIFORM_CAMERA_PROJ_MATRIX, Engine.ActiveCamera.Projection);
        Shader.SetUniform(UNIFORM_CAMERA_VIEW_MATRIX, Engine.ActiveCamera.View);
        Shader.SetUniform(UNIFORM_SINGLE_BUFFER_SIZE, sheet.SingleSpriteSize);

        Engine.GL.BindTextureUnit(0, sheet.Handle);
        Shader.SetUniform("uTexture", 0);
    }

    private unsafe void AggregateSpriteData(in ReadOnlySpan<Sprite> sprites)
    {
        if (dataPtr == null)
            return;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] is null)
                return; // incase we modified the array while itterating!! thanks multithreading

            dataPtr[i].modelMatrix = sprites[i].Transform.ModelMatrix;
            dataPtr[i].spriteOffset = sprites[i].GetFrameOffset();
            dataPtr[i].frameIndex = sprites[i].GetFrameIndex();
        }
    }
}
