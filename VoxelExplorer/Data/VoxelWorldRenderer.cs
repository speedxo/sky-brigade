using System.Numerics;
using System.Runtime.InteropServices;

using Horizon.Core;
using Horizon.Core.Data;
using Horizon.Engine;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;

using Silk.NET.OpenGL;


namespace VoxelExplorer.Data;

internal class VoxelWorldRenderer : Entity
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct VoxelVertex(Vector3 position, Vector2 uv)
    {
        [VertexLayout(0, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
        private readonly Vector3 Position = position;

        [VertexLayout(1, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
        private readonly Vector2 UV = uv;
    }


    private ChunkTechnique WorldTechnique;
    private VertexArrayObject QuadBuffer;
    private uint instanceCount;

    public VoxelWorld World { get; init; }

    public VoxelWorldRenderer(in VoxelWorld world)
    {
        World = world;
    }

    public override void Initialize()
    {
        base.Initialize();

        WorldTechnique = new ChunkTechnique();

        SetupBaseQuadBuffer();

        GameEngine.Instance.GL.BindVertexArray(QuadBuffer.Handle);
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.InstanceBuffer].Bind();
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.InstanceBuffer].VertexAttributeIPointer(2, 1, VertexAttribIType.Int, 4, 0);
        GameEngine.Instance.GL.VertexAttribDivisor(2, 1);
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.InstanceBuffer].Unbind();
        GameEngine.Instance.GL.BindVertexArray(0);

        for (int i = 0; i < World.Chunks.Length; i++)
        {
            var chunk = World.Chunks[i];
            var chunkData = World.Chunks[i].DataProvider;
            var instance_data = new List<VoxelInstanceData>();

            chunk.Buffer = GameEngine.Instance.ObjectManager.Buffers.Create(BufferObjectDescription.ArrayBuffer).Asset;

            GameEngine.Instance.GL.BindVertexArray(QuadBuffer.Handle);
            chunk.Buffer.Bind();
            chunk.Buffer.VertexAttributeIPointer(2, 1, VertexAttribIType.Int, 4, 0);
            GameEngine.Instance.GL.VertexAttribDivisor(2, 1);
            chunk.Buffer.Unbind();
            GameEngine.Instance.GL.BindVertexArray(0);

            for (int x = 0; x < Chunk.SIZE; x++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    for (int y = 0; y < Chunk.SIZE; y++)
                    {
                        if (chunkData[x, y - 1, z].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 0));
                        }
                        if (chunkData[x, y + 1, z].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 1));
                        }
                        if (chunkData[x + 1, y, z].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 2));
                        }
                        if (chunkData[x - 1, y, z].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 3));
                        }
                        if (chunkData[x, y, z - 1].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 4));
                        }
                        if (chunkData[x, y, z + 1].DataPack0 == 0)
                        {
                            instance_data.Add(VoxelInstanceData.Encode(x, y, z, 5));
                        }
                    }
                }
            }

            chunk.Buffer.NamedBufferData<VoxelInstanceData>(CollectionsMarshal.AsSpan(instance_data));

            chunk.BufferSize = instance_data.Count;
        }
    }

    private unsafe void SetupBaseQuadBuffer()
    {
        QuadBuffer = GameEngine.Instance.ObjectManager.VertexArrays.Create(new VertexArrayObjectDescription
        {
            Buffers = new()
            {
                { VertexArrayBufferAttachmentType.ArrayBuffer, BufferObjectDescription.ArrayBuffer },
                { VertexArrayBufferAttachmentType.ElementBuffer, BufferObjectDescription.ElementArrayBuffer},
                { VertexArrayBufferAttachmentType.InstanceBuffer, BufferObjectDescription.ArrayBuffer }
            }
        }).Asset;

        var quad_vertices = new VoxelVertex[] {
            new VoxelVertex(new Vector3(0, 0, 0), new Vector2(0, 0)),
            new VoxelVertex(new Vector3(1, 0, 0), new Vector2(1, 0)),
            new VoxelVertex(new Vector3(0, 0, 1), new Vector2(0, 1)),
            new VoxelVertex(new Vector3(1, 0, 1), new Vector2(1, 1)),
        };

        GameEngine.Instance.GL.BindVertexArray(QuadBuffer.Handle);
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer].Bind();
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer].SetLayout<VoxelVertex>();
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer].Unbind();

        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer].NamedBufferData(quad_vertices);

        var quad_indices = new uint[] { 0, 1, 2, 3 };
        QuadBuffer.Buffers[VertexArrayBufferAttachmentType.ElementBuffer].NamedBufferData(quad_indices);

        GameEngine.Instance.GL.BindVertexArray(0);
    }

    public override unsafe void Render(float dt, object? obj = null)
    {
        base.Render(dt, obj);

        WorldTechnique.Bind();
        GameEngine.Instance.GL.BindVertexArray(QuadBuffer.Handle);
        for (int i = 0; i < World.Chunks.Length; i++)
        {
            var chunk = World.Chunks[i];
            GameEngine.Instance.GL.BindVertexBuffer(2, chunk.Buffer.Handle, 4, 0);
            chunk.Buffer.Bind();
            chunk.Buffer.VertexAttributeIPointer(2, 1, VertexAttribIType.Int, 4, 0);
            GameEngine.Instance.GL.VertexAttribDivisor(2, 1);
            GameEngine.Instance.GL.DrawElementsInstanced(PrimitiveType.TriangleStrip, 4, DrawElementsType.UnsignedInt, null, (uint)chunk.BufferSize);
        }
        WorldTechnique.Unbind();
    }
}
