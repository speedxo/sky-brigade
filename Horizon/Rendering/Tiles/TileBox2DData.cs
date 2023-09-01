using Box2D.NetStandard.Dynamics.Fixtures;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID> where TTileID : Enum
    where TTextureID : Enum
{
    public struct TileBox2DData
    {
        public Fixture Fixture;
        public float Age;
        public float Distance;
    }
}