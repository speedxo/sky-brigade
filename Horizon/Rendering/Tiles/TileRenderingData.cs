using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
    where TTextureID : Enum
{
    public struct TileRenderingData
    {
        public TTextureID TextureID;
        public Vector3 Color;
        public bool IsVisible;

        public TileRenderingData()
        {
            IsVisible = true;
            Color = Vector3.One;
        }
    }
}
