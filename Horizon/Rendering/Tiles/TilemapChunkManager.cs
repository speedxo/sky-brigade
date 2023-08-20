using System.Numerics;
using System.Runtime.InteropServices;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Microsoft.Extensions.Options;
using Silk.NET.SDL;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TilemapChunkManager : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public TileMap Map { get; private set; }

        public TileMapChunk[] Chunks { get; private set; }

        public void Initialize()
        {
            Map = Parent as TileMap;

            Chunks = new TileMapChunk[TileMap.WIDTH];

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
            foreach (var chunk in Chunks)
                chunk.Populate(action);

            PostGenerateTiles();
        }

        private void PostGenerateTiles()
        {
            foreach (var chunk in Chunks)
                chunk.PostGenerate();
        }
    }

}