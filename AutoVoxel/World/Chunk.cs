using System;
using System.Diagnostics;
using System.Numerics;

using AutoVoxel.Data;
using AutoVoxel.Data.Chunks;

using Horizon.Core.Data;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Mesh;

using Silk.NET.Maths;

namespace AutoVoxel.World;

public class Chunk : IRenderable, IDisposable
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public IChunkData ChunkData { get; }

    public Vector2 Position { get; }
    public TessellatorMesh Mesh { get; }

    private uint elementCount = 0;
    private bool flagUpload;


    public Chunk(in Vector2 position)
    {
        this.Position = position;

        ChunkData = new LegacyChunkData();
        Mesh = new();
    }

    public void GenerateTree()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                int height = (int)(Perlin.perlin((x + Position.X * (WIDTH - 1)) * 0.05, 0.0, (z + Position.Y * (DEPTH - 1)) * 0.05) * HEIGHT);
                for (int y = HEIGHT - height; y > 0; y--)
                {
                    ChunkData[x, y, z] = new Tile { ID = TileID.Dirt };
                }
            }
        }
    }

    public void GenerateMesh(ChunkManager chunkManager)
    {
        Tessellator tes = new(Mesh);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    var tile = ChunkData[x, y, z];

                    // skip rendering if the current voxel is empty
                    if ((int)tile.ID < 1)
                        continue;

                    // check each face of the voxel for visibility
                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        // calculate the position of the neighboring voxel
                        Vector3 neighbourPosition = GetNeighborPosition(
                            new Vector3(x, y, z),
                            (CubeFace)faceIndex
                        );

                        var neighborTile = chunkManager[
                            (int)neighbourPosition.X,
                            (int)neighbourPosition.Y,
                            (int)neighbourPosition.Z
                        ];

                        // check if the neighboring voxel is empty or occludes the current voxel
                        if (neighborTile.ID == TileID.Air)
                        {
                            // generate the face if the neighboring voxel is empty
                            tes.AddCubeFace(GetOpposingFace((CubeFace)faceIndex), x, y, z);
                        }
                    }
                }
            }
        }
        Mesh.FlagDirty();
    }

    private CubeFace GetOpposingFace(CubeFace face)
    {
        return face switch
        {
            CubeFace.Left => CubeFace.Right,
            CubeFace.Right => CubeFace.Left,

            CubeFace.Front => CubeFace.Back,
            CubeFace.Back => CubeFace.Front,

            CubeFace.Top => CubeFace.Bottom,
            CubeFace.Bottom => CubeFace.Top
        };
    }

    private Vector3 GetNeighborPosition(Vector3 position, CubeFace face)
    {
        return face switch
        {
            CubeFace.Front => new Vector3(position.X, position.Y, position.Z + 1),
            CubeFace.Back => new Vector3(position.X, position.Y, position.Z - 1),

            CubeFace.Left => new Vector3(position.X - 1, position.Y, position.Z),
            CubeFace.Right => new Vector3(position.X + 1, position.Y, position.Z),

            CubeFace.Top => new Vector3(position.X, position.Y - 1, position.Z),
            CubeFace.Bottom => new Vector3(position.X, position.Y + 1, position.Z),
            _ => position,
        } + new Vector3(Position.X * (WIDTH), 0, Position.Y * (DEPTH));
    }
    public unsafe void Render(float dt, object? obj = null)
    {
        Mesh.Render(dt);
    }

    public void Dispose()
    {
        Mesh.Dispose();
    }
}
