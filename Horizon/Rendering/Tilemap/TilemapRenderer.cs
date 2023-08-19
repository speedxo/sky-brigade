using System;
using System.Xml.Linq;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering.Tilemap;

public class TilemapRenderer
{
    private static ShaderComponent _shader;
    public Dictionary<Spritesheet, TileMesh> SpritesheetMeshes { get; init; }
    public TileMapChunk Chunk { get; init; }

    public TilemapRenderer(TileMapChunk chunk)
    {
        _shader ??= new ShaderComponent(GameManager.Instance.ContentManager.LoadShader("Assets/tilemap_shaders/tiles.vert", "Assets/tilemap_shaders/tiles.frag"));

        this.Chunk = chunk;
        this.SpritesheetMeshes = new Dictionary<Spritesheet, TileMesh>();
    }

    public void GenerateMeshes(int slice)
    {
        var sheets = Chunk.SpritesheetTilePairs.Keys;

        foreach (var sheet in sheets)
        {
            if (!SpritesheetMeshes.ContainsKey(sheet))
                SpritesheetMeshes[sheet] = new TileMesh(_shader, sheet);

            SpritesheetMeshes[sheet].BeginMeshGeneration();
        }

        foreach ((Spritesheet sheet, Tile[] tiles) in Chunk.SpritesheetTilePairs)
        {
            SpritesheetMeshes[sheet].AddTiles(tiles, slice);
        }

        foreach (var sheet in sheets)
        {
            SpritesheetMeshes[sheet].EndMeshGeneration();
        }
    }

    public void Draw(float dt, RenderOptions? renderOptions = null)
    {
        foreach ((_, var mesh) in SpritesheetMeshes)
        {
            mesh.Draw(dt, renderOptions);
        }
    }
}

