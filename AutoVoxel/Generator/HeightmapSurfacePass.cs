using System.Numerics;
using System.Runtime.CompilerServices;

using Silk.NET.Maths;

namespace AutoVoxel.Generator;

public class HeightmapSurfacePass : IHeightmapGeneratorPass
{
    private readonly float frequency;

    public HeightmapSurfacePass(float frequency)
    {
        this.frequency = frequency;
    }

    public void Execute(ref float[] heightmap, int level, Vector2D<int> size, Vector2D<int> offset)
    {
        for (int i = 0; i < heightmap.Length; i++)
        {
            int globalX = i % size.X + offset.X;
            int globalY = i / size.Y + offset.Y;

            //heightmap[i] = (heightmap[i] + (float.Sin(globalX / (size.X * 0.1f)) + (float.Cos(globalY / (size.Y * 0.1f)))) / 2.0f);
            heightmap[i] = ((float)Perlin.OctavePerlin(globalX * 0.01 * level * frequency, 0.0, globalY * 0.01 * level * frequency, 5, 0.4));
        }
    }
}

public class RandomSurfacePass : IHeightmapGeneratorPass
{
    private readonly float strength, chance;
    private readonly Random random;

    public RandomSurfacePass(float strength, float chance)
    {
        // fixed seed for debugging.
        this.random = new Random(0);

        this.chance = chance;
        this.strength = strength;
    }

    public void Execute(ref float[] heightmap, int level, Vector2D<int> size, Vector2D<int> offset)
    {
        for (int i = 0; i < heightmap.Length; i++)
        {
            if (random.NextSingle() > chance) continue;

            heightmap[i] += (random.NextSingle() * strength);
        }
    }
}

public class SmoothSurfacePass : IHeightmapGeneratorPass
{
    private readonly float strength;

    public SmoothSurfacePass(float strength)
    {
        this.strength = strength;
    }

    // i absolutly did not write this but seeing it makes me appreciate how simple it really is
    private float Gaussian(float x, float y, float sigma)
    {
        float coefficient = 1.0f / (2.0f * (float)Math.PI * sigma * sigma);
        return coefficient * (float)Math.Exp(-(x * x + y * y) / (2.0f * sigma * sigma));
    }

    private float SampleHeightmap(in float[] heightmap, in Vector2D<int> size, int x, int y)
    {
        x = Math.Clamp(x, 0, size.X - 1);
        y = Math.Clamp(y, 0, size.Y - 1);

        return heightmap[x + y * size.X];
    }

    private float Smooth(in float[] heightmap, in Vector2D<int> size, int x, int y)
    {
        float sum = 0.0f;
        float totalWeight = 0.0f;

        float sigma = 1.0f; // TODO make adjustable

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int neighborX = x + dx;
                int neighborY = y + dy;

                if (neighborX >= 0 && neighborX < size.X && neighborY >= 0 && neighborY < size.Y)
                {
                    float weight = Gaussian(dx, dy, sigma) * strength;
                    sum += weight * SampleHeightmap(heightmap, size, neighborX, neighborY);
                    totalWeight += weight;
                }
            }
        }

        return sum / totalWeight;
    }

    public void Execute(ref float[] heightmap, int level, Vector2D<int> size, Vector2D<int> offset)
    {
        for (int i = 0; i < heightmap.Length; i++)
        {
            int x = i % size.X;
            int y = i / size.X;

            heightmap[i] = Smooth(heightmap, size, x, y);
        }
    }
}
