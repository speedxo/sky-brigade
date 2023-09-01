using Box2D.NetStandard.Dynamics.Fixtures;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID> where TTileID : Enum
    where TTextureID : Enum
{
    /// <summary>
    /// Contains complementary data for Box2D integration.
    /// </summary>
    public struct TileBox2DData
    {
        public Fixture Fixture;
        public float Age;
        public float Distance;
    }
}