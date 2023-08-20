using System.Numerics;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting;
using Horizon.Rendering.Spriting.Data;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TileMesh : Entity
    {
        public struct TileVertex
        {
            public Vector2 Position { get; set; }
            public Vector2 TexCoords { get; set; }

            public TileVertex(float x, float y, float uvX, float uvY)
            {
                this.TexCoords = new Vector2(uvX, uvY);
                this.Position = new Vector2(x, y);
            }

            public static readonly unsafe int SizeInBytes = (sizeof(Vector2) * 2);
        }

        private static int count = 0;
        public uint ElementCount { get; private set; }

        public ShaderComponent Shader { get; init; }
        public TileMap Map { get; init; }
        public VertexBufferObject<TileVertex> Vbo { get; private set; }
        public TileSet Set { get; init; }

        private List<TileVertex> _vertices;
        private List<uint> _indices;

        private uint _vertexCounter;

        public TileMesh(ShaderComponent shader, TileSet set, TileMap map)
        {
            this.Set = set;
            this.Shader = shader;
            this.Map = map;

            Vbo = new(GameManager.Instance.Gl);

            Vbo.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, (uint)TileVertex.SizeInBytes, 0);
            Vbo.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, (uint)TileVertex.SizeInBytes, 2 * sizeof(float));

            _indices = new();
            _vertices = new();
        }

        protected void Upload(TileVertex[] vertices, uint[] elements)
        {
            Vbo.VertexBuffer.BufferData(vertices);
            Vbo.ElementBuffer.BufferData(elements);

            ElementCount = (uint)elements.Length;
        }

        public void BeginMeshGeneration()
        {
            _vertexCounter = 0;
        }
        public void AddTiles(Tile[,] tiles, int slice)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tiles[x, y] == null) continue;
                    AddTile(tiles[x, y], slice);
                }
            }
        }


        private void AddTile(Tile tile, int slice)
        {
            uint[] getElements()
            {
                return new uint[] {
                _vertexCounter, _vertexCounter + 1, _vertexCounter + 2,
                _vertexCounter, _vertexCounter + 2, _vertexCounter + 3
            };
            }

            _vertices.AddRange(GetVertices(tile));
            _indices.AddRange(getElements());

            _vertexCounter += 4;
        }

        private static TileVertex[] GetVertices(Tile tile)
        {
            Vector2[] uv = tile.Set.GetTextureCoordinates(tile.RenderingData.TextureID);

            return new[] {
                new TileVertex(-Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[0].X, uv[0].Y),
                new TileVertex(Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[1].X, uv[1].Y),
                new TileVertex(Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, -Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[2].X, uv[2].Y),
                new TileVertex(-Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, -Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[3].X, uv[3].Y)
            };
        }

        public void EndMeshGeneration()
        {
            Upload(_vertices.ToArray(), _indices.ToArray());

            _vertices.Clear();
            _indices.Clear();
        }

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            if (ElementCount < 1) return; // Don't render if there is nothing to render to improve performance.

            var options = renderOptions ?? RenderOptions.Default;


            Shader.Use();

            GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
            GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Set.Texture.Handle);
            Shader.SetUniform("uTexture", 0);

            Shader.SetUniform("uView", options.Camera.View);
            Shader.SetUniform("uProjection", options.Camera.Projection);
            Shader.SetUniform("uWireframeEnabled", options.IsWireframeEnabled ? 1 : 0);

            Vbo.Bind();

            // Once again, I really don't want to make the whole method unsafe for one call.
            unsafe
            {
                // Turn on wireframe mode
                if (options.IsWireframeEnabled) GameManager.Instance.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

                //GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);
                GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);

                // Turn off wireframe mode
                if (options.IsWireframeEnabled) GameManager.Instance.Gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            }

            Vbo.Unbind();

            Shader.End();
        }

    }

}