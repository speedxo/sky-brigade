using AutoVoxel.Data;

namespace AutoVoxel.Data.Chunks;

public class Slice
{
    public const int SIZE = 32;

    public ISliceData SliceData { get; }

    public Tile this[in int x, in int y, in int z]
    {
        get => SliceData[x, y, z];
        set => SliceData[x, y, z] = value;
    }

    public Slice()
    {
        SliceData = new LegacySliceData();
    }
}
