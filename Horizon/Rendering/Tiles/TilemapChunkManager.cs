using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TilemapChunkManager : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public TileMap Map { get; private set; }

        public TileMapChunk[,] Chunks { get; private set; }

        public void Initialize()
        {
            Map = (TileMap)Parent!;

            Chunks = new TileMapChunk[TileMap.WIDTH, TileMap.HEIGHT];

            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y] = new TileMapChunk(Map!, new Vector2(x, y));
        }

        public void Update(float dt)
        {
            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y].Update(dt);
        }

        public void Draw(float dt, RenderOptions? options = null)
        {
            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y].Draw(dt, options);
        }

        public void GenerateMeshes()
        {
            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y].GenerateMesh();
        }

        /// <summary>
        /// This method accepts a generator function that is called for each position in the tilemap.
        /// </summary>
        /// <param name="generateTileFunc">The generator function</param>
        public void GenerateTiles(Func<TileMapChunk, Vector2, Tile?> generateTileFunc)
        {
            foreach (var chunk in Chunks)
                chunk.Generate(generateTileFunc);

            PostGenerateTiles();
        }

        /// <summary>
        /// This method accepts a populator action that is expected to fill the tile[].
        /// </summary>
        /// <param name="action">The populator action</param>
        public void PopulateTiles(Action<Tile?[,], TileMapChunk> action)
        {
            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y].Populate(action);

            PostGenerateTiles();
        }

        private void PostGenerateTiles()
        {
            for (int x = 0; x < TileMap.WIDTH; x++)
                for (int y = 0; y < TileMap.HEIGHT; y++)
                    Chunks[x, y].PostGenerate();
        }
    }
}