using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Horizon.Core.Data;
using Horizon.Engine;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Silk.NET.OpenGL;
using Shader = Horizon.OpenGL.Assets.Shader;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public readonly record struct TileRenderData(
        float X,
        float Y,
        float UvX,
        float UvY,
        Vector3 Color
    )
    {
        public static readonly uint SizeInBytes = sizeof(float) * 7;
    }

    public class TileMesh : GameObject
    {
        private const string UNIFORM_CAMERA_PROJ_MATRIX = "uCameraProjection";
        private const string UNIFORM_CAMERA_VIEW_MATRIX = "uCameraView";
        protected const string UNIFORM_TEXTURE_ALEBDO = "uTextureAlbedo";
        protected const string UNIFORM_TEXTURE_NORMAL = "uTextureNormal";
        public uint TileCount { get; private set; }

        /// <summary>
        /// If set to TileChunkCullMode.Top, all tiles above the screen midpoint are culled, and vice versa.
        /// </summary>
        public TileChunkCullMode CullMode { get; set; } = TileChunkCullMode.None;

        public Technique Shader { get; init; }
        public TileMap Map { get; init; }
        private VertexBufferObject Vbo { get; set; }
        public TileSet Set { get; init; }

        private readonly List<TileRenderData> _tileRenderData;

        private bool _uploadData,
            _isUpdatingMesh;

        private readonly struct BasicVertex
        {
            public readonly Vector2 Position
            {
                get => position;
                init => position = value;
            }

            public readonly Vector2 TexCoords
            {
                get => texCoords;
                init => texCoords = value;
            }

            public static uint SizeInBytes { get; } = sizeof(float) * 4;

            [VertexLayout(0, VertexAttribPointerType.Float)]
            private readonly Vector2 position;

            [VertexLayout(1, VertexAttribPointerType.Float)]
            private readonly Vector2 texCoords;

            public BasicVertex(Vector2 position, Vector2 texCoords)
            {
                Position = position;
                TexCoords = texCoords;
            }

            public BasicVertex(float x, float y, float uvX, float uvY)
            {
                Position = new Vector2(x, y);
                TexCoords = new Vector2(uvX, uvY);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="TileMesh" /> class.</summary>
        /// <param name="shader">The shader.</param>
        /// <param name="set">The set.</param>
        /// <param name="map">The map.</param>
        public TileMesh(in Technique shader, in TileSet set, in TileMap map)
        {
            Set = set;
            Shader = shader;
            Map = map;

            Vbo = new VertexBufferObject(
                Engine
                    .ObjectManager
                    .VertexArrays
                    .Create(
                        new OpenGL.Descriptions.VertexArrayObjectDescription
                        {
                            Buffers = new()
                            {
                                {
                                    VertexArrayBufferAttachmentType.ArrayBuffer,
                                    BufferObjectDescription.ArrayBuffer
                                },
                                {
                                    VertexArrayBufferAttachmentType.ElementBuffer,
                                    BufferObjectDescription.ElementArrayBuffer
                                },
                                {
                                    VertexArrayBufferAttachmentType.InstanceBuffer,
                                    BufferObjectDescription.ArrayBuffer
                                }
                            }
                        }
                    )
            );

            Vector2 tileTextureSize =
                set.TileSize / new Vector2(set.Material.Width, set.Material.Height);

            BasicVertex[] vertices = new BasicVertex[]
            {
                new BasicVertex(-Map.TileSize.X / 2, -Map.TileSize.Y / 2, 0, tileTextureSize.Y),
                new BasicVertex(
                    Map.TileSize.X / 2,
                    -Map.TileSize.Y / 2,
                    tileTextureSize.X,
                    tileTextureSize.Y
                ),
                new BasicVertex(Map.TileSize.X / 2, Map.TileSize.Y / 2, tileTextureSize.X, 0),
                new BasicVertex(-Map.TileSize.X / 2, Map.TileSize.Y / 2, 0, 0)
            };
            Vbo.VertexBuffer.BufferData(vertices);
            uint[] indices = new uint[] { 0, 1, 2, 0, 2, 3 };
            Vbo.ElementBuffer.BufferData(indices);

            Vbo.Bind();
            Vbo.VertexBuffer.Bind();
            Vbo.SetLayout<BasicVertex>();
            //Vbo.VertexAttributePointer(
            //    0,
            //    2,
            //    VertexAttribPointerType.Float,
            //    BasicVertex.SizeInBytes,
            //    0
            //);
            //Vbo.VertexAttributePointer(
            //    1,
            //    2,
            //    VertexAttribPointerType.Float,
            //    BasicVertex.SizeInBytes,
            //    2 * sizeof(float)
            //);
            Vbo.InstanceBuffer.Bind();

            Vbo.VertexAttributePointer(
                2,
                2,
                VertexAttribPointerType.Float,
                TileRenderData.SizeInBytes,
                0
            );
            Vbo.VertexAttributePointer(
                3,
                2,
                VertexAttribPointerType.Float,
                TileRenderData.SizeInBytes,
                2 * sizeof(float)
            );
            Vbo.VertexAttributePointer(
                4,
                3,
                VertexAttribPointerType.Float,
                TileRenderData.SizeInBytes,
                4 * sizeof(float)
            );
            Vbo.VertexAttributeDivisor(2, 1);
            Vbo.VertexAttributeDivisor(3, 1);
            Vbo.VertexAttributeDivisor(4, 1);

            Vbo.Unbind();
            Vbo.VertexBuffer.Unbind();

            _tileRenderData = new();
        }

        /// <summary>
        /// Constructs a mesh from a Span<Tile>. No null checking is performed.
        /// </summary>
        /// <param name="tiles">a span of tiles to generate the mesh from.</param>
        public void GenerateMeshFromTiles(in ReadOnlySpan<Tile> tiles)
        {
            if (_isUpdatingMesh)
                return;

            _isUpdatingMesh = true;

            int tileCount = 0;
            for (int i = 0; i < tiles.Length; i++)
                if (tiles[i].RenderingData.IsVisible)
                    tileCount++;

            _tileRenderData.Capacity = tileCount;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].RenderingData.IsVisible)
                    continue;

                _tileRenderData.Add(GenerateRenderData(in tiles[i]));
            }

            TileCount = (uint)tileCount;
            _uploadData = true;
        }

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        private static Tiling<TTextureID>.TileRenderData GenerateRenderData(in Tile tile)
        {
            Vector2 uv = tile switch
            {
                StaticTile sTile
                    => sTile.Set.GetNormalizedTextureCoordinatesFromTiledMapId(sTile.ID),
                _ => tile.Set.GetTextureCoordinates(tile.RenderingData.TextureID)[0]
            };

            return new TileRenderData(
                tile.GlobalPosition.X,
                tile.GlobalPosition.Y,
                uv.X,
                uv.Y,
                tile.RenderingData.Color
            );
        }

        public override void Render(float dt, object? obj = null)
        {
            if (_uploadData)
            {
                _uploadData = false;
                _isUpdatingMesh = false;

                Vbo.InstanceBuffer.NamedBufferData<TileRenderData>(
                    CollectionsMarshal.AsSpan(_tileRenderData)
                );

                _tileRenderData.Clear();
            }

            if (TileCount < 1)
                return;

            Shader.Bind();

            Set.Material.BindAttachment(MaterialAttachment.Albedo, 0);
            Shader.SetUniform(UNIFORM_TEXTURE_ALEBDO, 0);

            Set.Material.BindAttachment(MaterialAttachment.Normal, 1);
            Shader.SetUniform(UNIFORM_TEXTURE_NORMAL, 1);

            Shader.SetUniform(UNIFORM_CAMERA_PROJ_MATRIX, Engine.ActiveCamera.Projection);
            Shader.SetUniform(UNIFORM_CAMERA_VIEW_MATRIX, Engine.ActiveCamera.View);
            Shader.SetUniform("uDiscard", (int)CullMode);
            Shader.SetUniform("uClipOffset", Map.ClippingOffset);

            Vbo.Bind();
            Vbo.VertexBuffer.Bind();
            Vbo.ElementBuffer.Bind();

            unsafe // i don't wanna make the whole method unsafe just for this
            {
                Engine
                    .GL
                    .DrawElementsInstanced(
                        PrimitiveType.Triangles,
                        6,
                        DrawElementsType.UnsignedInt,
                        null,
                        TileCount
                    );
            }

            Vbo.Unbind();
            Shader.Unbind();
        }

        //public override void Load(in IMeshData<TileRenderData> data, in Material? mat = null)
        //{
        //    Vbo.InstanceBuffer.BufferData(data.Vertices.Span);
        //}
    }
}
