﻿using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon;
using Horizon.Data;
using Horizon.Extentions;
using Horizon.GameEntity.Components;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.OpenGL;
using Horizon.Primitives;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using static Horizon.Rendering.Tiling<Game2D.GameScene.TileID, Game2D.GameScene.TileTextureID>;

namespace Game2D;


public class GameScene : Scene
{
    public enum TileID
    {
        Dirt = 1
    }

    public enum TileTextureID
    {
        Dirt = 1,
        Grass = 2
    }

    private Player2D player;
    private SpriteBatch spriteBatch;
    private Camera cam;
    private TileMap tilemap;
    private World world;
    public static Box2DDebugDrawCallback debugDrawCallback;

    public GameScene()
    {
        InitializeGl();

        debugDrawCallback = new();
        debugDrawCallback.AppendFlags(DrawFlags.CenterOfMass | DrawFlags.Joint | DrawFlags.Pair);

        world = AddComponent<Box2DWorldComponent>();
        world.SetDebugDraw(debugDrawCallback);

        tilemap = new TileMap();
        AddEntity(tilemap);

        var sheet = tilemap.AddTileSet("tileset", new TileSet(GameManager.Instance.ContentManager.LoadTexture("content/tileset.png"), new Vector2(16)));
        sheet.RegisterTile(TileTextureID.Grass, new Vector2(16, 0));
        sheet.RegisterTile(TileTextureID.Dirt, new Vector2(16, 16));
        tilemap.PopulateTiles(PopulateTiles);
        tilemap.GenerateMeshes();

        AddEntity(player = new Player2D(world, tilemap));

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.AddSprite(player);
        spriteBatch.UpdateVBO();


        cam = new Camera()
        {
            Position = new Vector3(0, 0, 10)
        };

        InitializeRenderingPipeline();
    }
    private void PopulateTiles(Tile?[,] tiles, TileMapChunk chunk)
    {
        var noise = new NoiseGenerator(TileMap.WIDTH * TileMapChunk.Width, 1, new Vector2(1.0f));
        int heightOffset = (tiles.GetLength(1) - 1) / 4;

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            int height = (int)(noise.Data[x + chunk.Slice * TileMapChunk.Width, 0]
                            * ((tiles.GetLength(1) - 1) / 2.0f)
                            + heightOffset);

            for (int y = height; y > 0; y--)
            {
                tiles[x, y] = new DirtTile(chunk, new Vector2(x, y));
            }
        }
    }

    private static void InitializeGl()
    {
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.Texture2D);
        GameManager.Instance.Gl.Enable(EnableCap.Blend);
        GameManager.Instance.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    float cameraMovement = 10;
    public override void Update(float dt)
    {
        base.Update(dt);
        // Move camera with Q and E keys
        if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.Q))
        {
            cameraMovement += dt * 10.0f;
        }
        else if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Silk.NET.Input.Key.E))
        {
            cameraMovement += -dt * 10.0f;
        }

        cam.Position = new Vector3(player.Position.X, player.Position.Y, cameraMovement);

        cam.Update(dt);
    }
    public override void Draw(float dt, RenderOptions? renderOptions = null)
    {
        world.DrawDebugData();

        var options = (renderOptions ?? RenderOptions.Default) with
        {
            Camera = cam
        };
        base.Draw(dt, options);
    }

    public override void DrawOther(float dt, RenderOptions? renderOptions = null)
    {
        debugDrawCallback.Draw(dt, renderOptions);
    }
    public override void Dispose()
    {
        debugDrawCallback.Dispose();
    }

    public override void DrawGui(float dt)
    {
        ImGui.Text(player.Position.ToString());
        //ImGui.Text(tilemap[0, 0].GlobalPosition.ToString());
    }

}

