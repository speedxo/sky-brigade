using System.Numerics;

using AutoVoxel.Data;

using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Engine;

using Silk.NET.Maths;

namespace AutoVoxel.World;

public class ChunkManager : IGameComponent
{
    public VertexBufferPool BufferPool { get; }
    public Chunk[] Chunks { get; }

    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    public int Width { get; }
    public int Height { get; }

    public ChunkManager()
        : this(16, 16) { }

    public ChunkManager(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        Chunks = new Chunk[Width * Height];
        BufferPool = new();
    }

    public Tile this[int x, int y, int z]
    {
        get
        {
            if (x < 0 || y < 0 || z < 0)
                return Tile.OOB;

            int chunkX = x / (Chunk.WIDTH);
            int chunkY = z / (Chunk.DEPTH);

            if (chunkX > Width - 1 || chunkY > Height - 1 || y > Chunk.HEIGHT - 1)
                return Tile.OOB;

            int localX = x % (Chunk.WIDTH);
            int localZ = z % (Chunk.DEPTH);

            return Chunks[chunkX + chunkY * Width].ChunkData[localX, y, localZ];
        }
    }

    public void Initialize()
    {
        BufferPool.Initialize();

        Parallel.For(0, Width * Height, i =>
        {
            Chunks[i] = new Chunk(new Vector2(i % Width, i / Width));
            Chunks[i].GenerateTree();
        });
        Parallel.For(0, Width * Height, i =>
        {
            Chunks[i].GenerateMesh(this);
        });


        for (int i = 0; i < Width * Height; i++)
            Chunks[i].SetBuffer(BufferPool.Get());
    }

    public void Render(float dt, object? obj = null) { }

    public void UpdateState(float dt) { }

    public void UpdatePhysics(float dt) { }
}
