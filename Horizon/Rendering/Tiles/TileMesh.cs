using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTextureID>
{
    public class TileMesh : Entity
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

        public uint ElementCount { get; private set; }

        public ShaderComponent Shader { get; init; }
        public TileMap Map { get; init; }
        public VertexBufferObject<TileVertex> Vbo { get; private set; }
        public TileSet Set { get; init; }

        private readonly List<TileVertex> _vertices;
        private readonly List<uint> _indices;
        private uint _vertexCounter;
        private bool _uploadData,
            _isUpdatingMesh;

        public TileMesh(ShaderComponent shader, TileSet set, TileMap map)
        {
            Set = set;
            Shader = shader;
            Map = map;

            Vbo = new();

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
        }

        protected void Upload(ReadOnlySpan<TileVertex> vertices, ReadOnlySpan<uint> elements)
        {
            Vbo.VertexBuffer.BufferData(vertices);
            Vbo.ElementBuffer.BufferData(elements);

            ElementCount = (uint)elements.Length;
        }

        /// <summary>
        /// Constructs a mesh from a Span<Tile>. No null checking is performed.
        /// </summary>
        /// <param name="tiles">a span of tiles to generate the mesh from.</param>
        public void GenerateMeshFromTiles(ReadOnlySpan<Tile> tiles)
        {
            if (_isUpdatingMesh)
                return;

            _isUpdatingMesh = true;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tiles[i].RenderingData.IsVisible)
                    continue;

                AddTile(tiles[i]);
            }

            _uploadData = true;
        }

        [MethodImpl(
            MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization
        )]
        private void AddTile(in Tile tile)
        {
            static uint[] getElements(uint _offset) =>
                new uint[] { _offset, _offset + 1, _offset + 2, _offset, _offset + 2, _offset + 3 };

            _vertices.AddRange(GetVertices(tile));
            _indices.AddRange(getElements(_vertexCounter));
            _vertexCounter += 4;
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

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (_uploadData)
            {
                _uploadData = false;
                _isUpdatingMesh = false;

                Upload(CollectionsMarshal.AsSpan(_vertices), CollectionsMarshal.AsSpan(_indices));

                _indices.Clear();
                _vertices.Clear();
                _vertexCounter = 0;
            }

            if (ElementCount < 1)
            {
                return;
            }

            var options = renderOptions ?? RenderOptions.Default;

            Shader.Use();

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Set.Texture.Handle);
            Shader.SetUniform("uTexture", 0);

            Shader.SetUniform("uView", options.Camera.View);
            Shader.SetUniform("uProjection", options.Camera.Projection);
            Shader.SetUniform("uWireframeEnabled", options.IsWireframeEnabled ? 1 : 0);

            Vbo.Bind();

            if (options.IsWireframeEnabled)
            {
                GameManager.Instance.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            }

            unsafe // i dont wanna make the whole methud unsafe just for this
            {
                GameManager.Instance.Gl.DrawElements(
                    PrimitiveType.Triangles,
                    ElementCount,
                    DrawElementsType.UnsignedInt,
                    null
                );
            }

            if (options.IsWireframeEnabled)
            {
                GameManager.Instance.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }

            Vbo.Unbind();
            Shader.End();
        }
    }
}
