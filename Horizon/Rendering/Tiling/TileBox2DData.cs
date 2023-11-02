using Box2D.NetStandard.Dynamics.Fixtures;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
    where TTextureID : Enum
{
    /// <summary>
    /// Contains complementary data for Box2D integration. TODO: CHANGE!!!!
    /// </summary>
    public struct TilePhysicsData
    {
        public Fixture Fixture;
        public float Age;
        public float Distance;
        public bool HasCollider;
        public bool IsCollidable;
    }
}
