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
    public const int WIDTH = 16;
    public const int HEIGHT = 32;
    public const int DEPTH = 16;

    public IChunkData ChunkData { get; }

    public Vector2D<int> Position { get; }
    public VertexBufferObject Buffer { get; }

    private uint elementCount = 0;
    private List<Vertex3D> vertices = new List<Vertex3D>();
    private List<uint> indices = new List<uint>();
    private bool flagUpload;

    public Chunk(in VertexBufferObject vbo, in Vector2D<int> position)
    {
        this.Buffer = vbo;
        this.Position = position;

        ChunkData = new LegacyChunkData();
    }

    public void GenerateTree()
    {
        for (int i = 0; i < WIDTH * HEIGHT * DEPTH; i++)
        {
            ChunkData[i] = new Tile
            {
                ID = Random.Shared.NextSingle() > 0.5f ? TileID.Dirt : TileID.Air
            };
        }
    }

    public void GenerateMesh()
    {
        vertices.Clear();
        indices.Clear();

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

        for (int y = 0; y < HEIGHT - 1; y++)
        {
            for (int x = 0; x < WIDTH - 1; x++)
            {
                for (int z = 0; z < DEPTH - 1; z++)
                {
                    var tile = ChunkData[x, y, z];

                    // Skip rendering if the current voxel is empty (Air)
                    if (tile.ID == TileID.Air)
                        continue;

                    // Check each face of the voxel for visibility
                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        // Calculate the position of the neighboring voxel
                        Vector3 neighborPosition = GetNeighborPosition(
                            new Vector3(x, y, z),
                            (CubeFace)faceIndex
                        );

                        // Get the tile of the neighboring voxel
                        var neighborTile = ChunkData[
                            (int)neighborPosition.X,
                            (int)neighborPosition.Y,
                            (int)neighborPosition.Z
                        ];

                        // Check if the neighboring voxel is empty (Air) or occludes the current voxel
                        if (neighborTile.ID != TileID.Air)
                        {
                            // Generate the face if the neighboring voxel is empty
                            vertices.AddRange(
                                GenerateFace(
                                    (CubeFace)faceIndex,
                                    x + Position.X * WIDTH,
                                    y,
                                    z + Position.Y * DEPTH
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

    // Enum to represent cube faces
    public enum CubeFace
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }

    // Function to calculate the position of a neighboring voxel based on the cube face
    private Vector3 GetNeighborPosition(Vector3 position, CubeFace face)
    {
        return face switch
        {
            CubeFace.Front => new Vector3(position.X, position.Y, position.Z + 1),
            CubeFace.Back => new Vector3(position.X, position.Y, position.Z - 1),
            CubeFace.Left => new Vector3(position.X - 1, position.Y, position.Z),
            CubeFace.Right => new Vector3(position.X + 1, position.Y, position.Z),
            CubeFace.Top => new Vector3(position.X, position.Y + 1, position.Z),
            CubeFace.Bottom => new Vector3(position.X, position.Y - 1, position.Z),
            _ => position,
        };
    }

    private Vertex3D[] GenerateFace(CubeFace face, float x, float y, float z)
    {
        return face switch
        {
            CubeFace.Front => generateFrontFace(x, y, z),
            CubeFace.Back => generateBackFace(x, y, z),
            CubeFace.Top => generateTopFace(x, y, z),
            CubeFace.Bottom => generateBottomFace(x, y, z),
            CubeFace.Left => generateLeftFace(x, y, z),
            CubeFace.Right => generateRightFace(x, y, z),
            _ => Array.Empty<Vertex3D>(),
        };
    }

    private Vertex3D[] generateBackFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(
                new Vector3(0 + x, 0 + y, 1 + z),
                new Vector3(0, 0, -1),
                new Vector2(0, 1)
            ),
            new Vertex3D(
                new Vector3(1 + x, 0 + y, 1 + z),
                new Vector3(0, 0, -1),
                new Vector2(1, 1)
            ),
            new Vertex3D(
                new Vector3(1 + x, 1 + y, 1 + z),
                new Vector3(0, 0, -1),
                new Vector2(1, 0)
            ),
            new Vertex3D(new Vector3(0 + x, 1 + y, 1 + z), new Vector3(0, 0, -1), new Vector2(0, 0))
        };
    }

    private Vertex3D[] generateFrontFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(new Vector3(1 + x, 0 + y, 0 + z), new Vector3(0, 0, 1), new Vector2(0, 1)),
            new Vertex3D(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0, 0, 1), new Vector2(1, 1)),
            new Vertex3D(new Vector3(0 + x, 1 + y, 0 + z), new Vector3(0, 0, 1), new Vector2(1, 0)),
            new Vertex3D(new Vector3(1 + x, 1 + y, 0 + z), new Vector3(0, 0, 1), new Vector2(0, 0))
        };
    }

    private Vertex3D[] generateLeftFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(
                new Vector3(0 + x, 1 + y, 0 + z),
                new Vector3(-1, 0, 0),
                new Vector2(0, 1)
            ),
            new Vertex3D(
                new Vector3(0 + x, 0 + y, 0 + z),
                new Vector3(-1, 0, 0),
                new Vector2(1, 1)
            ),
            new Vertex3D(
                new Vector3(0 + x, 0 + y, 1 + z),
                new Vector3(-1, 0, 0),
                new Vector2(1, 0)
            ),
            new Vertex3D(new Vector3(0 + x, 1 + y, 1 + z), new Vector3(-1, 0, 0), new Vector2(0, 0))
        };
    }

    private Vertex3D[] generateRightFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(new Vector3(1 + x, 0 + y, 0 + z), new Vector3(1, 0, 0), new Vector2(0, 1)),
            new Vertex3D(new Vector3(1 + x, 1 + y, 0 + z), new Vector3(1, 0, 0), new Vector2(1, 1)),
            new Vertex3D(new Vector3(1 + x, 1 + y, 1 + z), new Vector3(1, 0, 0), new Vector2(1, 0)),
            new Vertex3D(new Vector3(1 + x, 0 + y, 1 + z), new Vector3(1, 0, 0), new Vector2(0, 0))
        };
    }

    private Vertex3D[] generateTopFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(new Vector3(0 + x, 0 + y, 0 + z), new Vector3(0, 1, 0), new Vector2(0, 1)),
            new Vertex3D(new Vector3(1 + x, 0 + y, 0 + z), new Vector3(0, 1, 0), new Vector2(1, 1)),
            new Vertex3D(new Vector3(1 + x, 0 + y, 1 + z), new Vector3(0, 1, 0), new Vector2(1, 0)),
            new Vertex3D(new Vector3(0 + x, 0 + y, 1 + z), new Vector3(0, 1, 0), new Vector2(0, 0))
        };
    }

    private Vertex3D[] generateBottomFace(float x, float y, float z)
    {
        return new[]
        {
            new Vertex3D(
                new Vector3(1 + x, 1 + y, 0 + z),
                new Vector3(0, -1, 0),
                new Vector2(0, 1)
            ),
            new Vertex3D(
                new Vector3(0 + x, 1 + y, 0 + z),
                new Vector3(0, -1, 0),
                new Vector2(1, 1)
            ),
            new Vertex3D(
                new Vector3(0 + x, 1 + y, 1 + z),
                new Vector3(0, -1, 0),
                new Vector2(1, 0)
            ),
            new Vertex3D(new Vector3(1 + x, 1 + y, 1 + z), new Vector3(0, -1, 0), new Vector2(0, 0))
        };
    }

    public void BufferData(in ReadOnlySpan<Vertex3D> vertices, in ReadOnlySpan<uint> elements)
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
