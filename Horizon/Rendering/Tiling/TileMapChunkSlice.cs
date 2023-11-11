namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMapChunkSlice
    {
        /// <summary>
        /// The width of the tile map chunk in tiles.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// The height of the tile map chunk in tiles.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Gets the 2D array of tiles in the chunk.
        /// </summary>
        public Tile?[] Tiles { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether this slice is rendered always on top regardless of parallaxing.
        /// </summary>
        public bool AlwaysOnTop { get; set; } = false;

        public TileMapChunkSlice(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Tiles = new Tile[Width * Height];
        }

        public Tile? this[int x, int y]
        {
            get { return Tiles[x + y * Width]; }
            set { Tiles[x + y * Width] = value; }
        }

        public Tile? this[int i]
        {
            get
            {
                if (i < 0 || i > Tiles.Length - 1)
                    return null;

                return Tiles[i];
            }
            set
            {
                if (i < 0 || i > Tiles.Length - 1)
                    throw new IndexOutOfRangeException();

                Tiles[i] = value;
            }
        }
    }
}
