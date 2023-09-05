global using static Horizon.Rendering.Tiling<Game2D.TileTextureID>;

using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon;
using Horizon.Extentions;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Rendering;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;

namespace Game2D;

public class GameScene : Scene
{
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

        if ((tilemap = TileMap.FromTiledMap(this, "content/maps/main.tmx")!) == null)
        {
            GameManager.Instance.Logger.Log(Horizon.Logging.LogLevel.Fatal, "Failed to load tilemap, aborting...");
            Environment.Exit(1);
        }
        else AddEntity(tilemap);

        //TileGenerator.RegisterTile<DirtTile>(TileID.Dirt);
        //TileGenerator.RegisterTile<CobblestoneTile>(TileID.Cobblestone);

        AddEntity(player = new Player2D(world, tilemap));

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.AddSprite(player);

        cam = new Camera()
        {
            Position = new Vector3(0, 0, 100)
        };

        InitializeRenderingPipeline();
    }

    //private void PopTiles(TileMapChunkSlice[] slices, TileMapChunk chunk)
    //{
    //    var slice = slices[0];

    //    int maxWidth = slice.Width - 1;
    //    int maxHeight = slice.Height - 1;

    //    for (int x = 0; x < maxWidth; x++)
    //    {
    //        int height = chunk.Position.Y == tilemap.Height - 1 ? (int)(maxHeight * noise[x + (int)chunk.Position.X * maxWidth]) : 0;

    //        for (int y = height; y < maxHeight; y++)
    //        {
    //            slice[x, maxHeight - y] = TileGenerator.GetTile(chunk, new Vector2(x, maxHeight - y), TileID.Dirt);
    //        }
    //    }

    //    slice = slices[1];

    //    for (int x = 0; x < maxWidth; x++)
    //    {
    //        int height = chunk.Position.Y == tilemap.Height - 1 ? (int)(maxHeight * noise[x + (int)chunk.Position.X * maxWidth]) : 0;

    //        for (int y = height; y < maxHeight; y += 2)
    //        {
    //            slice[x, maxHeight - y] = TileGenerator.GetTile(chunk, new Vector2(x, maxHeight - y), TileID.Cobblestone);
    //        }
    //    }
    //}

    private static void InitializeGl()
    {
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.Texture2D);
        GameManager.Instance.Gl.Enable(EnableCap.Blend);
        GameManager.Instance.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private float cameraMovement = 100;

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
        ImGui.Text(((int)player.Position.Y).ToString());
    }
}