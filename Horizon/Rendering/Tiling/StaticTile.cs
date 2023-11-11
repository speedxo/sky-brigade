using System.Numerics;

namespace Horizon.Rendering;

public partial class Tiling<TTextureID>
{
    /// <summary>
    /// An implementation of <see cref="Tile"/> used for rendering Tiled maps.
    /// </summary>
    public class StaticTile : Tile
    {
        public readonly struct TiledTileConfig
        {
            public readonly int ID { get; init; }
            public readonly TileSet Set { get; init; }
            public readonly bool IsCollectible { get; init; }
            public readonly bool IsVisible { get; init; }
            public readonly bool AlwaysOnTop { get; init; }
        }

        public int ID { get; init; }

        public StaticTile(TiledTileConfig config, TileMapChunk chunk, Vector2 local)
            : base(chunk, local)
        {
            this.ID = config.ID;
            this.Set = config.Set;
            this.PhysicsData.IsCollidable = config.IsCollectible;
            this.RenderingData.IsVisible = config.IsVisible;
        }
    }
}
