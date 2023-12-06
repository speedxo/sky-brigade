using System.Numerics;

using AutoVoxel.Data;

using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Engine;

using Silk.NET.Maths;

namespace AutoVoxel.World;

public class ChunkManager : IGameComponent
{
    public static VertexBufferPool BufferPool { get; private set; }
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

        BufferPool = new();
        Chunks = new Chunk[Width * Height];
    }


    // precomputed offsets for 8 neighboring chunks
    private static readonly Vector2[] neighbourOffsets = new Vector2[]
    {
        new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1),
        new Vector2(-1, 0), /* Center: (0, 0) */ new Vector2(1, 0),
        new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1)
    };

    public Chunk[] GetNeighbours(in Chunk chunk)
    {
        int x = (int)chunk.Position.X;
        int y = (int)chunk.Position.Y;

        Chunk[] neighbours = new Chunk[8]; // Assuming 8 neighbors

        int count = 0;

        foreach (Vector2 offset in neighbourOffsets)
        {
            int nx = x + (int)offset.X;
            int ny = y + (int)offset.Y;

            // Check bounds
            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
            {
                int index = ny * Width + nx;
                neighbours[count++] = Chunks[index];
            }
        }

        return count == neighbours.Length ? neighbours : neighbours.Take(count).ToArray();
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
    }

    public void Render(float dt, object? obj = null) { }

    public void UpdateState(float dt) { }

    public void UpdatePhysics(float dt) { }
}
