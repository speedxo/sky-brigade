using Horizon.GameEntity;
using Horizon.OpenGL;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shader = Horizon.Content.Shader;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public struct TileVertex
    {
        public Vector2 Position { get; set; }
        public Vector2 TexCoords { get; set; }
        public Vector3 Color { get; set; }

        public TileVertex(float x, float y, float uvX, float uvY, Vector3 color)
        {
            Color = new Vector3(color.X, color.Y, color.Z);
            TexCoords = new Vector2(uvX, uvY);
            Position = new Vector2(x, y);
        }

        public static readonly int SizeInBytes = sizeof(float) * 7;
    }

    public class TileMesh : Mesh<TileVertex>
    {
        protected const string UNIFORM_TEXTURE = "uTexture";
        public uint ElementCount { get; private set; }

        public Shader Shader { get; init; }
        public TileMap Map { get; init; }
        public VertexBufferObject<TileVertex> Vbo { get; private set; }
        public TileSet Set { get; init; }

        private readonly List<TileVertex> _vertices;
        private readonly List<uint> _indices;

        private bool _uploadData,
            _isUpdatingMesh;

        /// <summary>Initializes a new instance of the <see cref="TileMesh" /> class.</summary>
        /// <param name="shader">The shader.</param>
        /// <param name="set">The set.</param>
        /// <param name="map">The map.</param>
        public TileMesh(Shader shader, TileSet set, TileMap map)
        {
            Set = set;
            Shader = shader;
            Map = map;

            Vbo = new();
            Vbo.VertexArray.Bind();
            Vbo.VertexBuffer.Bind();

            Vbo.VertexAttributePointer(
                0,
                2,
                VertexAttribPointerType.Float,
                (uint)TileVertex.SizeInBytes,
                0
            );
            Vbo.VertexAttributePointer(
                1,
                2,
                VertexAttribPointerType.Float,
                (uint)TileVertex.SizeInBytes,
                2 * sizeof(float)
            );
            Vbo.VertexAttributePointer(
                2,
                3,
                VertexAttribPointerType.Float,
                (uint)TileVertex.SizeInBytes,
                4 * sizeof(float)
            );

            _indices = new();
            _vertices = new();
            Vbo.VertexArray.Unbind();
            Vbo.VertexBuffer.Unbind();
        }

        /// <summary>
        /// Constructs a mesh from a Span<Tile>. No null checking is performed.
        /// </summary>
        /// <param name="tiles">a span of tiles to generate the mesh from.</param>
        public void GenerateMeshFromTiles(in ReadOnlySpan<Tile> tiles)
        {
            uint _vertexCounter = 0;
            static uint[] getElements(uint _offset) =>
                new uint[] { _offset, _offset + 1, _offset + 2, _offset, _offset + 2, _offset + 3 };

            if (_isUpdatingMesh)
                return;

            _isUpdatingMesh = true;

            int tileCount = 0;
            for (int i = 0; i < tiles.Length; i++)
                if (tiles[i].RenderingData.IsVisible)
                    tileCount++;

            _vertices.Capacity = tileCount * 4;
            _indices.Capacity = tileCount * 6;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].RenderingData.IsVisible)
                    continue;

                _vertices.AddRange(GetVertices(tiles[i]));
                _indices.AddRange(getElements(_vertexCounter));
                _vertexCounter += 4;
            }

            ElementCount = (uint)_indices.Count;
            _uploadData = true;
        }

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        private static TileVertex[] GetVertices(in Tile tile)
        {
            Vector2[] uv;

            if (tile is StaticTile sTile)
                uv = sTile.Set.GetTextureCoordinatesFromTiledMapId(sTile.ID);
            else
                uv = tile.Set.GetTextureCoordinates(tile.RenderingData.TextureID);

            return new[]
            {
                new TileVertex(
                    -Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH,
                    Tile.TILE_HEIGHT / 2.0f - tile.GlobalPosition.Y * -Tile.TILE_HEIGHT,
                    uv[0].X,
                    uv[0].Y,
                    tile.RenderingData.Color
                ),
                new TileVertex(
                    Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH,
                    Tile.TILE_HEIGHT / 2.0f - tile.GlobalPosition.Y * -Tile.TILE_HEIGHT,
                    uv[1].X,
                    uv[1].Y,
                    tile.RenderingData.Color
                ),
                new TileVertex(
                    Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH,
                    -Tile.TILE_HEIGHT / 2.0f - tile.GlobalPosition.Y * -Tile.TILE_HEIGHT,
                    uv[2].X,
                    uv[2].Y,
                    tile.RenderingData.Color
                ),
                new TileVertex(
                    -Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH,
                    -Tile.TILE_HEIGHT / 2.0f - tile.GlobalPosition.Y * -Tile.TILE_HEIGHT,
                    uv[3].X,
                    uv[3].Y,
                    tile.RenderingData.Color
                )
            };
        }

        public override void Draw(float dt, ref RenderOptions options)
        {
            if (_uploadData)
            {
                _uploadData = false;
                _isUpdatingMesh = false;

                Vbo.VertexBuffer.BufferData(CollectionsMarshal.AsSpan(_vertices));
                Vbo.ElementBuffer.BufferData(CollectionsMarshal.AsSpan(_indices));

                _vertices.Clear();
                _indices.Clear();
            }

            if (ElementCount < 1)
            {
                return;
            }

            Shader.Use();

            Entity.Engine.GL.ActiveTexture(TextureUnit.Texture0);
            Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, Set.Texture.Handle);
            Shader.SetUniform(UNIFORM_TEXTURE, 0);

            Shader.SetUniform(UNIFORM_VIEW_MATRIX, options.Camera.View);
            Shader.SetUniform(UNIFORM_PROJECTION_MATRIX, options.Camera.Projection);
            Shader.SetUniform(UNIFORM_USE_WIREFRAME, options.IsWireframeEnabled ? 1 : 0);

            Vbo.VertexArray.Bind();

            if (options.IsWireframeEnabled)
            {
                Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            }

            unsafe // i dont wanna make the whole methud unsafe just for this
            {
                Entity.Engine.GL.DrawElements(
                    PrimitiveType.Triangles,
                    ElementCount,
                    DrawElementsType.UnsignedInt,
                    null
                );
            }

            if (options.IsWireframeEnabled)
            {
                Entity.Engine.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }

            Vbo.VertexArray.Unbind();
            Shader.End();
        }

        public override void Load(in IMeshData<TileVertex> data, in Material? mat = null)
        {
            Vbo.VertexBuffer.BufferData(data.Vertices.Span);
            Vbo.ElementBuffer.BufferData(data.Elements.Span);
        }

        public override void Dispose() { }
    }
}
