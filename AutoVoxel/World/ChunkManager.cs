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
            int chunkX = x / Chunk.WIDTH;
            int chunkY = y / Chunk.DEPTH;
            int localX = x % Chunk.WIDTH;
            int localY = y % Chunk.HEIGHT;
            int localZ = z % Chunk.DEPTH;

            if (
                chunkX >= 0
                && chunkX < Width
                && chunkY >= 0
                && chunkY < Height
                && localX >= 0
                && localX < Chunk.WIDTH
                && localY >= 0
                && localY < Chunk.HEIGHT
                && localZ >= 0
                && localZ < Chunk.DEPTH
            )
            {
                int chunkIndex = chunkX + chunkY * Width;
                return Chunks[chunkIndex].ChunkData[localX, localY, localZ];
            }

            return Tile.Empty;
        }
    }

    public void Initialize()
    {
        BufferPool.Initialize();

        var span = new Span<Chunk>(Chunks);

        for (int i = 0; i < Width * Height; i++)
        {
            span[i] = new Chunk(BufferPool.Get(), new Vector2D<int>(i % Width, i / Width));
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
