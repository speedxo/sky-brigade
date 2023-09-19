using Horizon.Rendering.Effects.Components;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TilemapRenderer
    {
        private readonly struct TileMapChunkSliceTileMeshes
        {
            public Dictionary<TileSet, Tile[]> TileSetPairs { get; init; }
            public Dictionary<TileSet, TileMesh> TileMeshPairs { get; init; }

            public TileMapChunkSlice Slice { get; init; }
            public TileMapChunkSliceTileMeshes(in TileMapChunkSlice slice)
            {
                this.Slice = slice;
                this.TileMeshPairs = new();
                this.TileSetPairs = new();
            }

            /// <summary>
            /// Updates the association between tile sets and their corresponding tiles in the chunk.
            /// </summary>
            /// <remarks>
            /// This method iterates through the tiles in the chunk and organizes them into pairs, where each tile set
            /// is associated with a 2D array of tiles that belong to that set. This allows for efficient rendering of tiles
            /// using tile sets and minimizes the number of draw calls required.
            /// </remarks>
            public void UpdateTileSetPairs()
            {
                // Clear the existing tile set pairs to rebuild them.
                TileSetPairs.Clear();

                // Temporary storage for organizing tiles by tile set.
                var tempPairs = new Dictionary<TileSet, List<Tile>>();

                // Iterate through the tiles in the chunk.
                for (int i = 0; i < Slice.Tiles.Length; i++)
                {
                    var tile = Slice[i];

                    if (tile is null)
                        continue;

                    // Check if the tile set is already in the temporary pairs.
                    if (!tempPairs.TryGetValue(tile.Set, out List<Tiling<TTextureID>.Tile>? value))
                    {
                        value = new List<Tile>();
                        // If not, create a new array for that tile set.
                        tempPairs[tile.Set] = value;
                    }

                    value.Add(tile);
                }

                // Copy the organized tile set pairs to the main TileSetPairs dictionary.
                foreach ((TileSet set, List<Tile> tiles) in tempPairs)
                {
                    TileSetPairs[set] = tiles.ToArray();
                    tiles.Clear();
                }

                // Clear the temporary storage to free up memory.
                tempPairs.Clear();
            }
        }

        private static ShaderComponent _shader;
        
        public TileMapChunk Chunk { get; init; }
        
        private Dictionary<TileMapChunkSlice, TileMapChunkSliceTileMeshes> TileMapChunkSliceTileMeshesKeyPairs { get; init; }

        public TilemapRenderer(in TileMapChunk chunk)
        {
            _shader ??= new ShaderComponent(
                GameManager.Instance.ContentManager.LoadShader(
                    "Assets/tilemap_shaders/tiles.vert",
                    "Assets/tilemap_shaders/tiles.frag"
                )
            );

            this.Chunk = chunk;
            this.TileMapChunkSliceTileMeshesKeyPairs = new();
        }

        
        /// <summary>
        /// Generates the mesh for the chunk.
        /// </summary>
        public void GenerateMesh()
        {
            foreach (var slice in Chunk.Slices)
            {
                if (!TileMapChunkSliceTileMeshesKeyPairs.ContainsKey(slice))
                    TileMapChunkSliceTileMeshesKeyPairs.Add(slice, new(slice));

                TileMapChunkSliceTileMeshesKeyPairs[slice].UpdateTileSetPairs();
            }

            foreach (var sliceMesh in TileMapChunkSliceTileMeshesKeyPairs.Values)
            {
                foreach (var tileset in sliceMesh.TileSetPairs.Keys)
                {
                    if (!sliceMesh.TileMeshPairs.ContainsKey(tileset))
                        sliceMesh.TileMeshPairs.Add(tileset, new TileMesh(_shader, tileset, Chunk.Map));

                    sliceMesh.TileMeshPairs[tileset].GenerateMeshFromTiles(sliceMesh.TileSetPairs[tileset]);
                }
            }
        }
        float _dirtyChunkUpdateTimer = 0.0f;
        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            var options = (renderOptions ?? RenderOptions.Default);

            Chunk.IsVisibleByCamera = options.Camera.Bounds.IntersectsWith(Chunk.Bounds);
            if (!Chunk.IsVisibleByCamera) return;

            _dirtyChunkUpdateTimer += dt;
            if (Chunk.IsDirty && _dirtyChunkUpdateTimer > 0.1f)
            {
                _dirtyChunkUpdateTimer = 0.0f;
                Chunk.IsDirty = false;
    
                GenerateMesh();
            }
            
            foreach ((_, var sliceMeshes) in TileMapChunkSliceTileMeshesKeyPairs)
            {
                foreach ((_, var mesh) in sliceMeshes.TileMeshPairs)
                {
                    mesh.Draw(dt, options);
                }
            }
        }
    }
}