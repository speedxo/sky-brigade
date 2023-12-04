using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using AutoVoxel.Data;
using AutoVoxel.Data.Chunks;

using Horizon.Core.Data;
using Horizon.Core.Primitives;
using Horizon.Engine;
using Horizon.OpenGL.Buffers;

using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace AutoVoxel.World;
public class Chunk : IRenderable
{
    public const int WIDTH = 32;
    public const int HEIGHT = 32;
    public const int DEPTH = 32;

    public IChunkData ChunkData { get; }

    public Vector2 Position { get; }
    public VertexBufferObject Buffer { get; }

    private uint elementCount = 0;
    private List<ChunkVertex> vertices = new List<ChunkVertex>();
    private List<uint> indices = new List<uint>();
    private bool flagUpload;

    public Chunk(in VertexBufferObject vbo, in Vector2 position)
    {
        this.Buffer = vbo;
        this.Position = position;

        ChunkData = new JaggedChunkData();
    }

    public void GenerateTree()
    {
        //for (int x = 0; x < WIDTH; x++)
        //{
        //    for (int z = 0; z < DEPTH; z++)
        //    {
        //        for (int y = 0; y < HEIGHT; y++)
        //        {
        //            ChunkData[x, y, z] = new Tile
        //            {
        //                ID = TileID.Dirt
        //            };
        //        }
        //    }
        //}
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
        uint indiciesCount = 0;

        void updateIndicies()
        {
            indices.AddRange(
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

        //int wth = WIDTH * HEIGHT, idx;
        //for (int i = 0; i < WIDTH * DEPTH * HEIGHT; i++)
        //{
        //    int z = i / wth;
        //    idx = i - z * wth;
        //    int y = idx / WIDTH;
        //    int x = idx % WIDTH;


        //}

        for (int x = 0; x < WIDTH; x++)
        {
            for (int z = 0; z < DEPTH; z++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    var tile = ChunkData[x, y, z];

                    // skip rendering if the current voxel is empty
                    if (tile.ID == TileID.Air)
                        continue;

                    // check each face of the voxel for visibility
                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        //// calculate the position of the neighboring voxel
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
                            vertices.AddRange(
                                GenerateFace(
                                    GetOpposingFace((CubeFace)faceIndex),
                                    x,
                                    y,
                                    z
                                )
                            );
                            updateIndicies();
                        }
                    }
                }
            }
        }
        flagUpload = true;
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

    private ChunkVertex[] GenerateFace(CubeFace face, int x, int y, int z)
    {
        if (x > 31 || y > 31 || z > 31) throw new Exception();

        return face switch
        {
            CubeFace.Front => generateFrontFace(x, y, z),
            CubeFace.Back => generateBackFace(x, y, z),
            CubeFace.Top => generateTopFace(x, y, z),
            CubeFace.Bottom => generateBottomFace(x, y, z),
            CubeFace.Left => generateLeftFace(x, y, z),
            CubeFace.Right => generateRightFace(x, y, z),
            _ => Array.Empty<ChunkVertex>(),
        };
    }

    private ChunkVertex[] generateBackFace(int x, int y, int z)
    {
        return new[]
       {
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomLeft),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.TopRight),
            new ChunkVertex(0 + x, 1 + y, 1 + z, CubeFace.Back, UVCoordinate.BottomRight)
        };
    }

    private ChunkVertex[] generateFrontFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomLeft),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.TopRight),
            new ChunkVertex(0 + x, 1 + y, 0 + z, CubeFace.Front, UVCoordinate.BottomRight)
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
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Left, UVCoordinate.TopRight),
            new ChunkVertex(1 + x, 1 + y, 1 + z, CubeFace.Left, UVCoordinate.BottomRight),
            new ChunkVertex(1 + x, 1 + y, 0 + z, CubeFace.Left, UVCoordinate.BottomLeft)
        };
    }

    private ChunkVertex[] generateTopFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(0 + x, 0 + y, 0 + z, CubeFace.Top, UVCoordinate.TopLeft),
            new ChunkVertex(1 + x, 0 + y, 0 + z, CubeFace.Top, UVCoordinate.BottomLeft),
            new ChunkVertex(1 + x, 0 + y, 1 + z, CubeFace.Top, UVCoordinate.TopRight),
            new ChunkVertex(0 + x, 0 + y, 1 + z, CubeFace.Top, UVCoordinate.BottomRight)
        };
    }

    private ChunkVertex[] generateBottomFace(int x, int y, int z)
    {
        return new[]
        {
            new ChunkVertex(
                1 + x, 0 + y, 0 + z,
                CubeFace.Bottom,
                UVCoordinate.TopLeft
            ),
            new ChunkVertex(
                0 + x, 0 + y, 0 + z,
                CubeFace.Bottom,
                UVCoordinate.BottomLeft
            ),
            new ChunkVertex(
                0 + x, 0 + y, 1 + z,
                CubeFace.Bottom,
                UVCoordinate.BottomRight
            ),
            new ChunkVertex(
                1 + x, 0 + y, 1 + z,
                CubeFace.Bottom,
                UVCoordinate.TopRight
                )
        };
    }

    public void BufferData(in ReadOnlySpan<ChunkVertex> vertices, in ReadOnlySpan<uint> elements)
    {
        this.Buffer.VertexBuffer.NamedBufferData(vertices);
        this.Buffer.ElementBuffer.NamedBufferData(elements);

        this.elementCount = (uint)elements.Length;
    }

    public unsafe void Render(float dt, object? obj = null)
    {
        if (flagUpload)
        {
            flagUpload = false;
            BufferData(CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(indices));

            vertices.Clear();
            indices.Clear();
        }

        if (elementCount < 1)
            return;

        Buffer.Bind();
        GameEngine
            .Instance
            .GL
            .DrawElements(
                PrimitiveType.Triangles,
                elementCount,
                DrawElementsType.UnsignedInt,
                null
            );
    }
}
