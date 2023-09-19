global using static Horizon.Rendering.Tiling<Game2D.TileTextureID>;

using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon;
using Horizon.Extentions;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Prefabs.Effects;
using Horizon.Rendering;
using Horizon.Rendering.Effects;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Silk.NET.OpenGL;
using System.Numerics;

using Game2D.Player;

namespace Game2D;

public class GameScene : Scene
{
    private Player2D player;
    private SpriteBatch spriteBatch;
    private Camera cam;
    private TileMap tilemap;
    private World world;

    private readonly Box2DDebugDrawCallback debugDrawCallback;

    public GameScene()
    {
        InitializeGl();

        debugDrawCallback = new();
        debugDrawCallback.Parent  =this;
        debugDrawCallback.Initialize();

        debugDrawCallback.AppendFlags(
            DrawFlags.CenterOfMass | DrawFlags.Joint | DrawFlags.Pair | DrawFlags.Shape
        );
        debugDrawCallback.Enabled = true;

        world = AddComponent<Box2DWorldComponent>();
        
        world.SetDebugDraw(debugDrawCallback);

        if ((tilemap = TileMap.FromTiledMap(this, "content/maps/main.tmx")!) == null)
        {
            GameManager.Instance.Logger.Log(
                Horizon.Logging.LogLevel.Fatal,
                "Failed to load tilemap, aborting..."
            );
            Environment.Exit(1);
        }
        else
            Entities.Add(tilemap);

        AddEntity(player = new Player2D(world, tilemap));

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.AddSprite(player);

        cam = new Camera() { Position = new Vector3(0, 0, 100) };

        InitializeRenderingPipeline();
    }

    protected override Effect[] GeneratePostProccessingEffects()
    {
        return new[] { new VingetteEffect() { Intensity = 1.0f } };
    }

    private static void InitializeGl()
    {
        GameManager.Instance.Gl.ClearColor(System.Drawing.Color.CornflowerBlue);
        GameManager.Instance.Gl.Enable(EnableCap.Texture2D);
        GameManager.Instance.Gl.Enable(EnableCap.Blend);
        GameManager.Instance.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private float cameraMovement = 10;

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

        var options = (renderOptions ?? RenderOptions.Default) with { Camera = cam };
        debugDrawCallback.Enabled = options.IsBox2DDebugDrawEnabled;
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

    public override void DrawGui(float dt) { 
        ImGui.Text(player.State.ToString());
    }
}
