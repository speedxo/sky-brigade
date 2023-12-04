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
        : this(8, 8) { }

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
            if (x < 0 || y < 0 || z < 0) return Tile.Empty;

            int chunkX = x / (Chunk.WIDTH);
            int chunkY = z / (Chunk.DEPTH);

            if (chunkX > Width - 1 || chunkY > Height - 1 || y > Chunk.HEIGHT - 1) return Tile.Empty;

            int localX = x % (Chunk.WIDTH);
            int localY = y;
            int localZ = z % (Chunk.DEPTH);

            int chunkIndex = chunkX + chunkY * Width;

            return Chunks[chunkIndex].ChunkData[localX, localY, localZ];
        }
    }

    public void Initialize()
    {
        BufferPool.Initialize();

        var span = new Span<Chunk>(Chunks);

        for (int i = 0; i < Width * Height; i++)
        {
            span[i] = new Chunk(BufferPool.Get(), new Vector2(i % Width, i / Width));
            span[i].GenerateTree();
        }
        for (int i = 0; i < Width * Height; i++)
        {
            span[i].GenerateMesh(this);
        }
    }

    public void Render(float dt, object? obj = null) { }

    public void UpdateState(float dt) { }

    public void UpdatePhysics(float dt) { }
}
