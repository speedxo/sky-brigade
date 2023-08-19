using System.Numerics;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Microsoft.Extensions.Options;

namespace Horizon.Rendering.Tilemap;

public class TilemapChunkManager : IGameComponent
{
    public string Name { get; set; }
    public Entity Parent { get; set; }
    public Tilemap Map { get; private set; }

    public TileMapChunk[] Chunks { get; private set; }

    public void Initialize()
    {
        Map = Parent as Tilemap;

        Chunks = new TileMapChunk[Tilemap.WIDTH];

        for (int i = 0; i < Chunks.Length; i++)
            Chunks[i] = new TileMapChunk(Map, i);
    }

    public void Update(float dt)
    {

    }

    public void Draw(float dt, RenderOptions? options = null)
    {
        for (int i = 0; i < Chunks.Length; i++)
            Chunks[i].Draw(dt, options);
    }

    public void GenerateMeshes()
    {
        for (int i = 0; i < Chunks.Length; i++)
            Chunks[i].GenerateMesh();
    }

    public void GenerateTiles(Func<Vector2, Vector2, Tile> generateTileFunc)
    {
        foreach (var chunk in Chunks)
            chunk.Generate(generateTileFunc);

        PostGenerateTiles();
    }

    private void PostGenerateTiles()
    {
        foreach (var chunk in Chunks)
            chunk.PostGenerate();
    }
}

