global using static Horizon.Rendering.Tiling<TileBash.TileTextureID>;
using System;
using System.Drawing;
using System.Numerics;
using Bogz.Logging;
using Bogz.Logging.Loggers;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon;
using Horizon.Core;
using Horizon.Engine;
using Horizon.GameEntity;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Rendering;
using Horizon.Rendering.Particles;
using Horizon.Rendering.Particles.Materials;
using Horizon.Rendering.Spriting;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using TileBash.Animals;
using TileBash.Player;

namespace TileBash;

public class GameScene : Scene
{
    public override Camera ActiveCamera { get; protected set; }

    private Random random;
    private Player2D player;
    private SpriteBatch spriteBatch;
    private Camera2D cam;
    private TileMap tilemap;
    private World world;
    private ParticleRenderer2D rainParticleSystem;
    private DeferredRenderer2D deferredRenderer;

    //private RenderOptions charOptions;
    private int catCounter = 0;

    //private readonly Box2DDebugDrawCallback debugDrawCallback;
    public GameScene()
    {
        if ((tilemap = TileMap.FromTiledMap(this, "content/maps/main.tmx")!) == null)
        {
            ConcurrentLogger.Instance.Log(LogLevel.Fatal, "Failed to load tilemap, aborting...");
            Environment.Exit(1);
        }
        tilemap.ParallaxIndex = 2;
        tilemap.ClippingOffset = 0.1f;
    }

    public override void Initialize()
    {
        base.Initialize();

        InitializeGl();

        random = new Random(Environment.TickCount);

        //debugDrawCallback = new();
        //debugDrawCallback.Parent = this;
        //debugDrawCallback.Initialize();
            
        //debugDrawCallback.AppendFlags(
        //    DrawFlags.CenterOfMass | DrawFlags.Joint | DrawFlags.Pair | DrawFlags.Shape
        //);
        //debugDrawCallback.Enabled = true;

        world = AddComponent<Box2DWorldComponent>();
        //world.SetDebugDraw(debugDrawCallback);

        AddEntity(player = new Player2D(world, tilemap));
        deferredRenderer = AddEntity<DeferredRenderer2D>(
            new((uint)Engine.WindowManager.WindowSize.X, (uint)Engine.WindowManager.WindowSize.Y)
        );
        cam = AddEntity<Camera2D>(new(deferredRenderer.ViewportSize / 4.0f));
        this.ActiveCamera = cam;
        deferredRenderer.AddEntity(tilemap);

        spriteBatch = tilemap.AddEntity<SpriteBatch>();
        spriteBatch.Add(player);

        deferredRenderer.AddEntity(
            rainParticleSystem = new ParticleRenderer2D(100_000)
            {
                MaxAge = 2.5f,
                StartColor = new Vector3(4, 0, 255) / new Vector3(255),
                EndColor = new Vector3(66, 135, 245) / new Vector3(255),
                Enabled = true
            }
        );

        // The tile map will now render this entity between the parallax layers, implicitly.
        //tilemap.SetParallaxEntity(spriteBatch);

        AddEntity(
            new IntervalRunner(
                1 / 25.0f,
                () =>
                {
                    (float, float) roll(int diag) =>
                        (
                            random.NextSingle() * Engine.WindowManager.WindowSize.X + diag / 2.0f,
                            random.NextSingle() * Engine.WindowManager.WindowSize.Y + diag / 2.0f
                        );

                    for (int diagonal = 0; diagonal < 4; diagonal++)
                    {
                        (var x, var y) = roll(diagonal); // slight bias
                        SpawnParticle(cam.ScreenToWorld(new Vector2(x, y)), -Vector2.One, 0.2f);
                    }
                }
            )
        );

        base.Initialize();
    }

    //protected override Effect[] GeneratePostProccessingEffects()
    //{
    //    return new[] { new VingetteEffect() { Intensity = 1.0f } };
    //}

    private void InitializeGl()
    {
        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        Engine.GL.Enable(EnableCap.Texture2D);
        Engine.GL.Enable(EnableCap.Blend);
        Engine.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private float cameraMovement = 1f;

    public override void UpdateState(float dt)
    {
        //if (Engine.InputManager.KeyboardManager.IsKeyPressed(Key.F3))
        //    Engine.Debugger.Enabled = !Engine.Debugger.Enabled;
        if (Engine.InputManager.KeyboardManager.IsKeyPressed(Key.E))
            cameraMovement = Math.Clamp(cameraMovement - 2, 0, 32);
        else if (Engine.InputManager.KeyboardManager.IsKeyPressed(Key.Q))
            cameraMovement = Math.Clamp(cameraMovement + 2, 1, 32);

        cam.Zoom = cameraMovement < 2 ? 1 : 2 * MathF.Round((cameraMovement) / 2);

        if (Engine.InputManager.KeyboardManager.IsKeyPressed(Key.G))
        {
            catCounter += 128;
        }
        if (Engine.InputManager.KeyboardManager.IsKeyPressed(Key.F))
        {
            SpawnCircle();
        }
        if (
            Engine
                .InputManager
                .MouseManager
                .GetData()
                .Actions
                .HasFlag(Horizon.Input.VirtualAction.PrimaryAction)
        )
        {
            var mouseData = Engine.InputManager.MouseManager.GetData();
            SpawnParticle(
                cam.ScreenToWorld(mouseData.Position),
                new Vector2(1.0f - mouseData.Direction.X, mouseData.Direction.Y)
            );
        }

        base.UpdateState(dt);

        cam.Position = new Vector3(player.Position.X, player.Position.Y, 0.0f);
    }

    public override void Render(float dt, object? obj = null)
    {
        base.Render(dt);

        //world.DrawDebugData();

        if (catCounter > 0)
        {
            Cat[] gattos = new Cat[catCounter];
            for (int i = 0; i < catCounter; i++)
            {
                var x = random.NextSingle() * Engine.WindowManager.WindowSize.X;
                var y = random.NextSingle() * Engine.WindowManager.WindowSize.Y;

                var cat = new Cat();
                cat.Transform.Position = cam.ScreenToWorld(new Vector2(x, y));

                gattos[i] = AddEntity(cat);
            }
            spriteBatch.AddRange(gattos);
            catCounter = 0;
        }

        //debugDrawCallback.Enabled = options.IsBox2DDebugDrawEnabled;
        // spriteBatch.RenderImplicit = true;
    }

    private void SpawnParticle(Vector2 pos, Vector2 dir, float blend = 0.5f)
    {
        float val = ((random.NextSingle() * 2.0f) - MathF.PI);

        rainParticleSystem.Add(
            new Particle2D(
                new Vector2(MathF.Sin(val), MathF.Cos(val)) * (1.0f - blend) + dir * blend,
                pos
            )
        );
    }

    private void SpawnCircle(int count = 250)
    {
        float val = 0.0f;
        for (int i = 0; i < count; i++)
        {
            val = random.NextSingle() * MathF.PI * 2.0f;

            rainParticleSystem.Add(
                new Particle2D(new Vector2(MathF.Cos(val), MathF.Sin(val)), player.Position)
            );
        }
    }

    //public override void DrawGui(float dt)
    //{
    //    if (ImGui.Begin("Particles (& cats)"))
    //    {
    //        ImGui.DragFloat("Clipping Offset", ref tilemap.ClippingOffset, 0.01f, -1, 1);

    //        ImGui.Text($"Particles: {rainParticleSystem.Count}");
    //        ImGui.Text($"Cats: {spriteBatch.Count - 1}");

    //        if (ImGui.Button("Spawn 100000"))
    //            SpawnCircle(100000);
    //        ImGui.End();
    //    }
    //}
}
