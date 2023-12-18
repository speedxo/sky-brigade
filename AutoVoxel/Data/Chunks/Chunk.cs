using System;
using System.Diagnostics;
using System.Numerics;

using AutoVoxel.Generator;
using AutoVoxel.Rendering;
using AutoVoxel.World;

using Horizon.Core.Data;
using Horizon.Core.Primitives;
using Horizon.Engine;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Mesh;

using Silk.NET.Maths;

namespace AutoVoxel.Data.Chunks;

public class Chunk : IRenderable, IDisposable
{
    public const int SLICES = 4;
    public const int WIDTH = Slice.SIZE;
    public const int HEIGHT = Slice.SIZE * SLICES;
    public const int DEPTH = Slice.SIZE;

    public Slice[] Slices { get; }
    public Vector2 Position { get; }
    public TessellatorMesh VoxelMesh { get; }
    public TessellatorMesh FolliageMesh { get; }

    private uint elementCount = 0;
    private bool flagUpload;

    public Tile this[int x, int y, int z]
    {
        get => y >= HEIGHT ? Tile.OOB : y < 0 ? Tile.OOB : Slices[y / Slice.SIZE][x, y % Slice.SIZE, z];
        set
        {
            if (y >= HEIGHT || y < 0) return;

            Slices[y / Slice.SIZE][x, y % Slice.SIZE, z] = value;
        }
    }

    public Chunk(in Vector2 position)
    {
        Position = position;
        Slices = new Slice[SLICES];
        for (int i = 0; i < SLICES; i++)
            Slices[i] = new();

        VoxelMesh = new();
        FolliageMesh = new();
    }

    public void GenerateTree(in HeightmapGenerator generator)
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                int height = (int)(generator[(int)(x + Position.X * (WIDTH - 1)), (int)(z + Position.Y * (DEPTH - 1))] * (HEIGHT - 5));

                for (int y = height; y > 0; y--)
                {
                    if (Perlin.OctavePerlin((x + Position.X * (WIDTH - 1)) * 0.05, y * 0.05, (z + Position.Y * (DEPTH - 1)) * 0.05, 2, 0.5) > 0.7)
                    {
                        int localY = height - y;

                        if (y == height && Random.Shared.NextSingle() > 0.8f)
                            this[x, height + 1, z] = new Tile { ID = TileID.Grass };

                        this[x, y, z] = new Tile { ID = localY < 6 ? TileID.Dirt : TileID.Stone };
                    }
                }
            }
        }
    }

    public void GenerateMesh(ChunkManager chunkManager)
    {
        Tessellator tes = new(VoxelMesh);
        Tessellator folTes = new(FolliageMesh);

        for (int x = 0; x < WIDTH; x++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    var tile = this[x, y, z];

                    // skip rendering if the current voxel is empty
                    if ((int)tile.ID < 2)
                        continue;

                    // hack in grass
                    if (tile.ID == TileID.Grass)
                    {
                        folTes.AddCross(new Tile { ID = TileID.Grass }, (int)x, (int)y, (int)z);
                        continue;
                    }

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
                        if (neighborTile.ID == TileID.Air || neighborTile.ID == TileID.Grass)
                        {
                            // generate the face if the neighboring voxel is empty
                            tes.AddCubeFace(GetOpposingFace((CubeFace)faceIndex), tile, x, y, z);
                        }
                    }
                }
            }
        }
        VoxelMesh.FlagDirty();
        FolliageMesh.FlagDirty();
    }

    private CubeFace GetOpposingFace(CubeFace face)
    {
        return face switch
        {
            CubeFace.Left => CubeFace.Right,
            CubeFace.Right => CubeFace.Left,

            CubeFace.Front => CubeFace.Back,
            CubeFace.Back => CubeFace.Front,

            CubeFace.Top => CubeFace.Top,
            CubeFace.Bottom => CubeFace.Bottom
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
        } + new Vector3(Position.X * WIDTH, 0, Position.Y * DEPTH);
    }
    public unsafe void Render(float dt, object? obj = null)
    {
        VoxelMesh.Render(dt);

    }
    public unsafe void RenderFolliage(float dt, object? obj = null)
    {
        FolliageMesh.Render(dt);
    }

    public void Dispose()
    {
        VoxelMesh.Dispose();
    }
}
