﻿using System;
using System.Xml.Linq;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Effects.Components;
using Horizon.Rendering.Spriting;

namespace Horizon.Rendering;

public abstract partial class Tiling<TTileID, TTextureID>
{
    public class TilemapRenderer
    {
        private static ShaderComponent _shader;
        public Dictionary<TileSet, TileMesh> TileSetMeshes { get; init; }
        public TileMapChunk Chunk { get; init; }

        public TilemapRenderer(TileMapChunk chunk)
        {
            _shader ??= new ShaderComponent(GameManager.Instance.ContentManager.LoadShader("Assets/tilemap_shaders/tiles.vert", "Assets/tilemap_shaders/tiles.frag"));

            this.Chunk = chunk;
            this.TileSetMeshes = new();
        }

        public void GenerateMeshes(int slice)
        {
            var sheets = Chunk.TileSetPairs.Keys;

            foreach (var sheet in sheets)
            {
                if (!TileSetMeshes.ContainsKey(sheet))
                    TileSetMeshes[sheet] = new TileMesh(_shader, sheet, Chunk.Map);

                TileSetMeshes[sheet].BeginMeshGeneration();
            }

            foreach ((TileSet set, Tile[,] tiles) in Chunk.TileSetPairs)
            {
                TileSetMeshes[set].AddTiles(tiles, slice);
            }

            foreach (var sheet in sheets)
            {
                TileSetMeshes[sheet].EndMeshGeneration();
            }
        }

        public void Draw(float dt, RenderOptions? renderOptions = null)
        {
            foreach ((_, var mesh) in TileSetMeshes)
            {
                mesh.Draw(dt, renderOptions);
            }
        }
    }

}