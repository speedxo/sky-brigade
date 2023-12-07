namespace AutoVoxel.Generator;

public class HeightmapSurfacePass : IHeightmapGeneratorPass
{
    public byte Level { get; set; } = 1;

    public void Execute(ref float[] heightmap, int w, int h)
    {
        for (int i = 0; i < heightmap.Length; i++)
        {
            int globalX = i % w;
            int globalY = i / w;

            int chunkX = globalX / 32;
            int chunkY = globalY / 32;

            heightmap[i] = (float.Sin(globalX / 32.0f) + float.Cos(globalY / 32.0f) + 1.0f) / 2.0f;
        }
    }
}
