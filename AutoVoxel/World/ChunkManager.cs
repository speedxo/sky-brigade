using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Engine;

using Silk.NET.Maths;

namespace AutoVoxel.World;

public class ChunkManager : IGameComponent
{
    public VertexBufferPool BufferPool { get; }
    public Dictionary<int, Chunk> Chunks { get; }

    public bool Enabled { get; set; }
    public string Name { get; set; }
    public Entity Parent { get; set; }

    public ChunkManager()
    {
        Chunks = new();
        BufferPool = new();
    }

    public void Initialize()
    {
        BufferPool.Initialize();
    }

    public void Render(float dt, object? obj = null)
    {
    }

    public void UpdateState(float dt)
    {
        int chunkX = (int)(GameEngine.Instance.SceneManager.CurrentInstance.ActiveCamera.Position.X / Chunk.WIDTH);
        int chunkY = (int)(GameEngine.Instance.SceneManager.CurrentInstance.ActiveCamera.Position.Y / Chunk.DEPTH);
        int chunkIndex = chunkX + chunkY * Chunk.WIDTH;

        if (!Chunks.ContainsKey(chunkIndex))
        {
            var chunk = new Chunk(BufferPool.Get(), new Vector2D<int>(chunkX, chunkY));
            chunk.GenerateTree();
            chunk.GenerateMesh();
            Chunks.Add(chunkIndex, chunk);
        }
    }

    public void UpdatePhysics(float dt)
    {

    }
}
