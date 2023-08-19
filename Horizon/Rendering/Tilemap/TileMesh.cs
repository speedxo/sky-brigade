using System.Numerics;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting;
using Horizon.Rendering.Spriting.Data;
using Silk.NET.OpenGL;

namespace Horizon.Rendering.Tilemap;

public class TileMesh : Entity
{
    private static int count = 0;
    public uint ElementCount { get; private set; }

    public ShaderComponent Shader { get; init; }
    public VertexBufferObject<Vertex2D> Vbo { get; private set; }
    public Spritesheet Sheet { get; init; }

    private List<Vertex2D> _vertices;
    private List<uint> _indices;

    private uint _vertexCounter;

    public TileMesh(ShaderComponent shader, Spritesheet sheet)
    {
        this.Sheet = sheet;
        this.Shader = shader;

        Vbo = new(GameManager.Instance.Gl);

        Vbo.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, (uint)Vertex2D.SizeInBytes, 0);
        Vbo.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, (uint)Vertex2D.SizeInBytes, 2 * sizeof(float));
        Vbo.VertexAttributePointer(2, 1, VertexAttribPointerType.Int, (uint)Vertex2D.SizeInBytes, 4 * sizeof(float));

        if (count++ == 0)
            GameManager.Instance.Debugger.GeneralDebugger.AddWatch("TileMap Count", () => count);
    }

    protected void Upload(Vertex2D[] vertices, uint[] elements)
    {
        Vbo.VertexBuffer.BufferData(vertices);
        Vbo.ElementBuffer.BufferData(elements);

        ElementCount = (uint)elements.Length;
    }

    public void BeginMeshGeneration()
    {
        _vertices = new();
        _indices = new();

        _vertexCounter = 0;
    }

    public void AddTiles(Tile[] tiles, int slice)
    {
        for (int x = 0; x < tiles.Length; x++)
        {
            AddTile(tiles[x], slice);
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

        _vertices.AddRange(GetVertices(tile, slice));
        _indices.AddRange(getElements());

        _vertexCounter += 4;
    }

    private Vertex2D[] GetVertices(Tile tile, int slice)
    {
        Vector2[] uv = tile.Sheet.GetTextureCoordinates(tile.RenderingData.FrameName);

        int id = Sheet.GetNewSpriteId();

        return new Vertex2D[] {
                new Vertex2D(-Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[0].X, uv[0].Y, id),
                new Vertex2D(Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[1].X, uv[1].Y, id),
                new Vertex2D(Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, -Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[2].X, uv[2].Y, id),
                new Vertex2D(-Tile.TILE_WIDTH / 2.0f + tile.GlobalPosition.X * Tile.TILE_WIDTH, -Tile.TILE_HEIGHT / 2.0f- tile.GlobalPosition.Y* Tile.TILE_HEIGHT, uv[3].X, uv[3].Y, id)
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
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Sheet.Texture.Handle);
        Shader.SetUniform("uTexture", 0);

        Shader.SetUniform("uView", options.Camera.View);
        Shader.SetUniform("uProjection", options.Camera.Projection);

        Vbo.Bind();

        // Once again, I really don't want to make the whole method unsafe for one call.
        unsafe
        {
            GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);
        }

        Vbo.Unbind();

        Shader.End();
    }

}

