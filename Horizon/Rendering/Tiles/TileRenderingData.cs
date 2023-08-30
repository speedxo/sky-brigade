namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID> where TTileID : Enum
    where TTextureID : Enum
{
    public struct TileRenderingData
    {
        public TTextureID TextureID;
        public System.Drawing.Color Color;

        public TileRenderingData()
        {
            Color = System.Drawing.Color.White;
        }
    }
}

