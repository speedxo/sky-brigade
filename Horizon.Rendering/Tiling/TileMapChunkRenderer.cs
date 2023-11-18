using Horizon.Content;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMapChunkRenderer : IGameComponent
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

        private static Technique _shader;
        private float _dirtyChunkUpdateTimer = 0.0f;

        public TileMapChunk Chunk { get; init; }

        private Dictionary<
            TileMapChunkSlice,
            TileMapChunkSliceTileMeshes
        > TileMapChunkSliceTileMeshesKeyPairs { get; init; }

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public Entity Parent { get; set; }

        public TileMapChunkRenderer(in TileMapChunk chunk)
        {
            this.Chunk = chunk;
            this.TileMapChunkSliceTileMeshesKeyPairs = new();
        }

        public void Render(float dt, object? obj = null)
        {
            _dirtyChunkUpdateTimer += dt;
            if (Chunk.IsDirty && _dirtyChunkUpdateTimer > 0.1f)
            {
                _dirtyChunkUpdateTimer = 0.0f;
                Chunk.IsDirty = false;

                GenerateMesh();
            }

            foreach (var (_, sliceMeshes) in TileMapChunkSliceTileMeshesKeyPairs)
            {
                foreach (var (_, mesh) in sliceMeshes.TileMeshPairs)
                {
                    mesh.Render(dt);
                }
            }
        }

        public void Initialize()
        {
            _shader ??= new Technique(
                GameObject
                    .Engine
                    .ContentManager
                    .Shaders
                    .Create(ShaderDescription.FromPath("shaders/tilemap", "tilemap"))
                    .Asset
            );
        }

        public void UpdateState(float dt) { }

        public void UpdatePhysics(float dt) { }

        /// <summary>d
        /// Generates the mesh for the chunk.
        /// </summary>
        /// <remarks>
        /// The method first updates all tileset pairs for each slice of each chunk, then uses the tileset/tile associations to generate a mesh for each tileset so that multiple tilesets can exist within the same slice/chunk/map.
        /// </remarks>
        public void GenerateMesh()
        {
            // UpdateState tileset/tile associations.
            foreach (var slice in Chunk.Slices)
            {
                if (!TileMapChunkSliceTileMeshesKeyPairs.ContainsKey(slice))
                    TileMapChunkSliceTileMeshesKeyPairs.Add(slice, new(slice));

                TileMapChunkSliceTileMeshesKeyPairs[slice].UpdateTileSetPairs();
            }

            // generate the meshes accordingly.
            foreach (var sliceMesh in TileMapChunkSliceTileMeshesKeyPairs.Values)
            {
                foreach (var tileset in sliceMesh.TileSetPairs.Keys)
                {
                    if (!sliceMesh.TileMeshPairs.ContainsKey(tileset))
                        sliceMesh
                            .TileMeshPairs
                            .Add(tileset, new TileMesh(_shader, tileset, Chunk.Map));

                    sliceMesh
                        .TileMeshPairs[tileset]
                        .GenerateMeshFromTiles(sliceMesh.TileSetPairs[tileset]);
                }
            }
        }

        public void DrawSliceAtIndex(int index, float dt, in TileChunkCullMode cullMode)
        {
            //Chunk.IsVisibleByCamera = options.Camera.Bounds.IntersectsWith(Chunk.Bounds);
            //if (!Chunk.IsVisibleByCamera)
            //    return; TODO: reimplement

            _dirtyChunkUpdateTimer += dt;
            if (Chunk.IsDirty && _dirtyChunkUpdateTimer > 0.1f)
            {
                _dirtyChunkUpdateTimer = 0.0f;
                Chunk.IsDirty = false;

                GenerateMesh();
            }
            if (!TileMapChunkSliceTileMeshesKeyPairs.Any())
                return;

            foreach (
                var (_, mesh) in TileMapChunkSliceTileMeshesKeyPairs[
                    Chunk.Slices[index]
                ].TileMeshPairs
            )
            {
                mesh.CullMode = cullMode;
                mesh.Render(dt);
                mesh.CullMode = TileChunkCullMode.None;
            }
        }
    }
}
