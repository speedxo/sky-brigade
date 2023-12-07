using AutoVoxel.Data;
using AutoVoxel.Data.Chunks;

namespace AutoVoxel.Generator;

public class HeightmapGenerator
{
    private float[] heightmap;
    private readonly int sizeX, sizeY;
    private IHeightmapGeneratorPass[][] passes;

    public HeightmapGenerator(in ChunkManager manager)
    {
        heightmap = new float[Chunk.WIDTH * manager.Width * Chunk.DEPTH * manager.Height];
        Array.Fill(heightmap, 0.5f);
        sizeX = Chunk.WIDTH * manager.Width;
        sizeY = Chunk.DEPTH * manager.Height;

        passes = new IHeightmapGeneratorPass[][] {
            new IHeightmapGeneratorPass[]
            {
                new HeightmapSurfacePass(1.5f),
                new RandomSurfacePass(0.01f, 0.3f),
                new SmoothSurfacePass(1.0f)
            }

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
                    // Calculate the starting index for copying from the heightmap to the target array
                    int sourceStartIndex = x * targetSizeX + y * sizeX * targetSizeY;

                    // Copy the subarray from the heightmap to the target array
                    for (int dx = 0; dx < targetSizeX; dx++)
                    {
                        for (int dy = 0; dy < targetSizeY; dy++)
                        {
                            target[dx + dy * targetSizeX] = heightmap[sourceStartIndex + dx + dy * sizeX];
                        }
                    }


                    // Execute the terrain generation pass on the target array
                    for (int j = 0; j < passes[i].Length; j++)
                    {
                        passes[i][j].Execute(ref target, (i + 1), new Silk.NET.Maths.Vector2D<int>(targetSizeX, targetSizeY), new Silk.NET.Maths.Vector2D<int>(x * targetSizeX, y * targetSizeY));
                    }

                    // Copy the modified subarray from the target array back to the heightmap
                    for (int dx = 0; dx < targetSizeX; dx++)
                    {
                        for (int dy = 0; dy < targetSizeY; dy++)
                        {
                            heightmap[sourceStartIndex + dx + dy * sizeX] = target[dx + dy * targetSizeX];
                        }
                    }
                }
            }
        }
    }

    public float this[int x, int z]
    {
        get => heightmap[x % sizeX + z * sizeY];
    }
}
