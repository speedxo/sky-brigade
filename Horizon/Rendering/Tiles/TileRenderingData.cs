using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID> where TTileID : Enum
    where TTextureID : Enum
{
    public struct TileRenderingData
    {
        public TTextureID TextureID;
        public Vector3 Color;

        public TileRenderingData()
        {
            Color = Vector3.One;
        }
    }
}