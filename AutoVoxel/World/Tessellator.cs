using AutoVoxel.Data;

namespace AutoVoxel.World;

/// <summary>
/// Helper class to add vertices to a voxel mesh.
/// </summary>
public class Tessellator
{
    public TessellatorMesh Mesh { get; init; }

    private uint indiciesCount = 0;

    public Tessellator(in TessellatorMesh mesh)
    {
        Mesh = mesh;
    }

    public void AddCubeFace(in CubeFace face, in int x, in int y, in int z)
    {
        if (x > 31 || y > 31 || z > 31) throw new Exception();

        Mesh.Vertices.AddRange(face switch
        {
            CubeFace.Front => generateFrontFace(x, y, z),
            CubeFace.Back => generateBackFace(x, y, z),
            CubeFace.Top => generateTopFace(x, y, z),
            CubeFace.Bottom => generateBottomFace(x, y, z),
            CubeFace.Left => generateLeftFace(x, y, z),
            CubeFace.Right => generateRightFace(x, y, z),
            _ => Array.Empty<ChunkVertex>(),
        });
        updateIndicies();
    }


    void updateIndicies()
    {
        Mesh.Indices.AddRange(
            new uint[]
            {
                    indiciesCount + 0,
                    indiciesCount + 1,
                    indiciesCount + 2,
                    indiciesCount + 0,
                    indiciesCount + 2,
                    indiciesCount + 3
            }
        );

        indiciesCount += 4;
    }

    private ChunkVertex[] generateBackFace(int x, int y, int z)
    {
        return new[]
       {
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomRight),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomLeft)
        };
    }

    private ChunkVertex[] generateFrontFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.TopLeft),
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomRight),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomLeft)
        };
    }

    private ChunkVertex[] generateRightFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Right, UVCoordinate.TopLeft),
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Right, UVCoordinate.TopRight),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Right, UVCoordinate.BottomRight),
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Right, UVCoordinate.BottomLeft)
        };
    }


    private ChunkVertex[] generateLeftFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Left, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Left, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Left, UVCoordinate.BottomRight),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Left, UVCoordinate.BottomLeft)
        };
    }

    private ChunkVertex[] generateTopFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Top, UVCoordinate.TopLeft),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Top, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Top, UVCoordinate.BottomRight),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Top, UVCoordinate.BottomLeft)
        };
    }

    private ChunkVertex[] generateBottomFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Top, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Top, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Top, UVCoordinate.BottomRight),
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Top, UVCoordinate.BottomLeft)
        };
    }
}
