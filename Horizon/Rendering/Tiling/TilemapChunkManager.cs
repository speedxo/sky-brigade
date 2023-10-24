using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System;
using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMapChunkManager : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public TileMap Map { get; private set; }

        public TileMapChunk[] Chunks { get; private set; }

        public TileMapChunk? this[int index]
        {
            get
            {
                if (index < 0 || index > Chunks.Length - 1)
                    return null;

                return Chunks[index];
            }
        }
        public TileMapChunk? this[int x, int y]
        {
            get
            {
                int index = x + y * Map.Width;

                if (index < 0 || index > Chunks.Length - 1)
                    return null;

                return Chunks[index];
            }
        }

        public void Initialize()
        {
            Map = (TileMap)Parent!;

            Chunks = new TileMapChunk[Map.Width * Map.Height];

            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                    Chunks[x + y * Map.Width] = new TileMapChunk(Map, new Vector2(x, y));
        }

        public void Update(float dt)
        {
            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                    Chunks[x + y * Map.Width].Update(dt);
        }

        public void Draw(float dt, ref RenderOptions options)
        {
            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                    Chunks[x + y * Map.Width].Draw(dt, ref options);
        }

        public void GenerateMeshes()
        {
            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++) 
                    Chunks[x + y * Map.Width].Renderer.GenerateMesh();
        }

        /// <summary>
        /// This method accepts a populator action that is expected to fill the tile[].
        /// </summary>
        /// <param name="action">The populator action</param>
        public void PopulateTiles(Action<TileMapChunkSlice[], TileMapChunk> action)
        {
            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                    Chunks[x + y * Map.Width].Populate(action);

            PostGenerateTiles();
        }

        internal void PostGenerateTiles()
        {
            for (int x = 0; x < Map.Width; x++)
                for (int y = 0; y < Map.Height; y++)
                    Chunks[x + y * Map.Width].PostGenerate();
        }
    }
}
