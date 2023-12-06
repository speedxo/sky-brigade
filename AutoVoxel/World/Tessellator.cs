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

    public void AddCubeFace(in CubeFace face, in Tile tile, in int x, in int y, in int z)
    {
        Mesh.Vertices.AddRange(face switch
        {
            CubeFace.Front => generateFrontFace(tile, x, y, z),
            CubeFace.Back => generateBackFace(tile, x, y, z),
            CubeFace.Top => generateTopFace(tile, x, y, z),
            CubeFace.Bottom => generateBottomFace(tile, x, y, z),
            CubeFace.Left => generateLeftFace(tile, x, y, z),
            CubeFace.Right => generateRightFace(tile, x, y, z),
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

    private ChunkVertex[] generateBackFace(Tile tile, int x, int y, int z)
    {
        return new[]
       {
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.TopLeft, id: tile.ID),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomLeft, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.TopRight, id : tile.ID)
        };
    }

    private ChunkVertex[] generateFrontFace(Tile tile, int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.TopLeft, id: tile.ID),
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomLeft, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.TopRight, id : tile.ID)
        };
    }

    private ChunkVertex[] generateRightFace(Tile tile, int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Right, UVCoordinate.TopLeft, id : tile.ID),
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Right, UVCoordinate.TopRight, id : tile.ID),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Right, UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Right, UVCoordinate.BottomLeft, id : tile.ID)
        };
    }


    private ChunkVertex[] generateLeftFace(Tile tile, int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Left, UVCoordinate.TopLeft, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Left, UVCoordinate.BottomLeft, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Left, UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Left, UVCoordinate.TopRight, id : tile.ID)
        };
    }

    private ChunkVertex[] generateTopFace(Tile tile, int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Top,  UVCoordinate.TopLeft, id: tile.ID),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Top,  UVCoordinate.BottomLeft, id : tile.ID),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Top,  UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Top,  UVCoordinate.TopRight, id : tile.ID)
        };
    }

    private ChunkVertex[] generateBottomFace(Tile tile, int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Bottom, UVCoordinate.TopLeft, id: tile.ID),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Bottom, UVCoordinate.TopRight, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Bottom, UVCoordinate.BottomRight, id : tile.ID),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Bottom, UVCoordinate.BottomLeft, id : tile.ID)
        };
    }
}
