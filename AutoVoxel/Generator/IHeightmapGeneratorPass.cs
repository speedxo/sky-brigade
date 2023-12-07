using Silk.NET.Maths;

namespace AutoVoxel.Generator;

public interface IHeightmapGeneratorPass
{
    void Execute(ref float[] heightmap, int level, Vector2D<int> size, Vector2D<int> offset);
}
