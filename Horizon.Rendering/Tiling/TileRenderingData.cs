using System.Numerics;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
    where TTextureID : Enum
{
    /// <summary>
    /// A data structure to store useful visual debugging properties.
    /// </summary>
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
