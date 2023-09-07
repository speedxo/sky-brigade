using Horizon.Rendering.Effects.Components;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TilemapRenderer
    {
        private static ShaderComponent _shader;
        public Dictionary<TileSet, TileMesh> TileSetMeshes { get; init; }
        public TileMapChunk Chunk { get; init; }

        public TilemapRenderer(TileMapChunk chunk)
        {
            _shader ??= new ShaderComponent(
                GameManager.Instance.ContentManager.LoadShader(
                    "Assets/tilemap_shaders/tiles.vert",
                    "Assets/tilemap_shaders/tiles.frag"
                )
            );

            this.Chunk = chunk;
            this.TileSetMeshes = new();
        }

        public void GenerateMeshes()
        {
            var sheets = Chunk.TileSetPairs.Keys;

            foreach (var sheet in sheets)
            {
                if (!TileSetMeshes.ContainsKey(sheet))
                    TileSetMeshes[sheet] = new TileMesh(_shader, sheet, Chunk.Map);
            }

            foreach ((TileSet set, Tile[] tiles) in Chunk.TileSetPairs)
            {
                TileSetMeshes[set].GenerateMeshFromTiles(tiles);
            }
        }

        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            var options = (renderOptions ?? RenderOptions.Default);
            Chunk.IsVisibleByCamera = options.Camera.Bounds.IntersectsWith(Chunk.Bounds);

            //if (!Chunk.IsVisibleByCamera) return;

            foreach ((_, var mesh) in TileSetMeshes)
            {
                mesh.Draw(dt, renderOptions);
            }
        }
    }
}
