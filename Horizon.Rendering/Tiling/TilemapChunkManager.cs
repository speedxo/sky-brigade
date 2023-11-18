using System.Drawing.Drawing2D;
using System.Numerics;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMapChunkManager : IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }
        public bool Enabled { get; set; }
        public TileMap Map { get; private set; }
        public TileMapChunk[] Chunks { get; private set; }

        private float _secondTimer = 1.1f;
        private bool _drawParallax = false;

        public TileMapChunkManager(in TileMap map)
        {
            Map = map;

            Chunks = new TileMapChunk[Map.Width * Map.Height];

            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i] = new TileMapChunk(Map, new Vector2(i % Map.Width, i / Map.Width));
        }

        public void Initialize()
        {
            for (int i = 0; i < Map.Width * Map.Height; i++)
                Chunks[i].Initialize();
        }

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

        public void UpdateState(float dt)
        {
            _secondTimer += dt;
            if (_secondTimer >= 1)
            {
                _secondTimer = 0;
                // slow
            }
        }

        public void UpdatePhysics(float dt) { }

        /// <summary>
        /// Renders all the chunk slices starting from 0 and ending at the layer specified by renderClampLower.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="options">The options.</param>
        public void RenderLower(in int renderClampLower, float dt)
        {
            // render background chunks
            for (int _layerIndex = 0; _layerIndex < renderClampLower; _layerIndex++)
                RenderSlices(_layerIndex, dt, TileChunkCullMode.None);
        }

        /// <summary>
        /// Renders all the chunk slices starting at the layer specified by renderClampUpper and ending at the final layer.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="options">The options.</param>
        public void RenderUpper(in int renderClampUpper, float dt)
        {
            // render background chunks
            for (int _layerIndex = renderClampUpper; _layerIndex < Map.Depth; _layerIndex++)
                RenderSlices(_layerIndex, dt, TileChunkCullMode.Bottom);
        }

        private void RenderSlices(int index, in float dt, in TileChunkCullMode cullMode)
        {
            for (int chunkIndex = 0; chunkIndex < Map.Width * Map.Height; chunkIndex++)
            {
                Chunks[chunkIndex]?.RenderSlice(index, dt, cullMode);
            }
        }

        public void Render(float dt, object? obj = null)
        {
            // not used
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
