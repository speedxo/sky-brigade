namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID> where TTileID : Enum
    where TTextureID : Enum
{
    public class TileRenderingData
    {
        public TTextureID TextureID { get; set; }
    }
}

