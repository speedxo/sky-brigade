using System;
using System.Numerics;

namespace Horizon.Rendering
{
	public partial class Tiling<TTextureID>
	{
        public class StaticTile : Tile
        {
            public struct TiledTileConfig
            {
                public int ID { get; set; }
                public TileSet Set { get; set; }
                public bool IsCollidable { get; set; }
                public bool IsVisible { get; set; }
            }

            public int ID { get; init; }

            public StaticTile(TiledTileConfig config, TileMapChunk chunk, Vector2 local)
                :base(chunk, local)
            {
                this.ID = config.ID;
                this.Set = config.Set;
                this.PhysicsData.IsCollidable = true;
                this.RenderingData.IsVisible = config.IsVisible;
            }
        }
    }
}

