using AutoVoxel.Data;
using AutoVoxel.Data.Chunks;

namespace AutoVoxel.Generator;

public class HeightmapGenerator
{
    private float[] heightmap;
    private readonly int sizeX, sizeY;
    private IHeightmapGeneratorPass[] passes;

    public HeightmapGenerator(in ChunkManager manager)
    {
        heightmap = new float[Chunk.WIDTH * manager.Width * Chunk.DEPTH * manager.Height];
        sizeX = Chunk.WIDTH * manager.Width;
        sizeY = Chunk.DEPTH * manager.Height;

        passes = new IHeightmapGeneratorPass[] {
            new HeightmapSurfacePass()
        };
    }

    public void Generate()
    {
        for (int i = 0; i < passes.Length; i++)
        {
            int targetSizeX = sizeX / (i + 1);
            int targetSizeY = sizeY / (i + 1);

            float[] target = new float[targetSizeX * targetSizeY];
            for (int x = 0; x < sizeX / targetSizeX; x++)
            {
                for (int y = 0; y < sizeY / targetSizeY; y++)
                {
                    Array.Copy(heightmap, x * targetSizeX + y * targetSizeX * targetSizeY, target, 0, targetSizeX * targetSizeY);
                    passes[i].Execute(ref target, targetSizeX, targetSizeY);
                    Array.Copy(target, 0, heightmap, x * targetSizeX + y * targetSizeX * targetSizeY, targetSizeX * targetSizeY);
                }
            }
        }
    }

    public float this[int x, int z]
    {
        get
        {
            return heightmap[x % sizeX + z * sizeY];
        }
    }
}
