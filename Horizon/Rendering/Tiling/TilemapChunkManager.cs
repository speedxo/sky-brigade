using Horizon.GameEntity;
using Horizon.GameEntity.Components;
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

            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i] = new TileMapChunk(Map, new Vector2(i % Map.Width, i / Map.Width));
        }

        public void UpdateState(float dt)
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].UpdateState(dt);
        }

        public void UpdatePhysics(float dt) { }

        public void Render(float dt, ref RenderOptions options)
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].Render(dt, ref options);
        }

        public void GenerateMeshes()
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].Renderer.GenerateMesh();
        }

        /// <summary>
        /// This method accepts a populator action that is expected to fill the tile[].
        /// </summary>
        /// <param name="action">The populator action</param>
        public void PopulateTiles(Action<TileMapChunkSlice[], TileMapChunk> action)
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].Populate(action);

            PostGenerateTiles();
        }

        internal void PostGenerateTiles()
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].PostGenerate();
        }
    }
}
