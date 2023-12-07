namespace AutoVoxel.Generator;

public interface IHeightmapGeneratorPass
{
    public byte Level { get; protected set; }

    void Execute(ref float[] heightmap, int width, int height);
}
