global using static Horizon.Rendering.Tiling<TileBash.TileTextureID>;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Horizon;
using Horizon.Extentions;
using Horizon.GameEntity.Components.Physics2D;
using Horizon.Prefabs.Effects;
using Horizon.Rendering;
using Horizon.Rendering.Effects;
using Horizon.Rendering.Particles;
using Horizon.Rendering.Particles.Materials;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using System.Numerics;
using TileBash.Animals;
using TileBash.Player;

namespace TileBash;

public class GameScene : Scene
{
    private readonly Random random;
    private Player2D player;
    private SpriteBatch spriteBatch;
    private readonly Camera cam;
    private readonly TileMap tilemap;
    private readonly World world;
    private ParticleRenderer2D particles;
    private RenderOptions charOptions;
    private int catCounter = 0;
    private readonly Box2DDebugDrawCallback debugDrawCallback;

    public GameScene()
    {
        InitializeGl();

        random = new Random(Environment.TickCount);

        debugDrawCallback = new();
        debugDrawCallback.Parent = this;
        debugDrawCallback.Initialize();

        debugDrawCallback.AppendFlags(
            DrawFlags.CenterOfMass | DrawFlags.Joint | DrawFlags.Pair | DrawFlags.Shape
        );
        debugDrawCallback.Enabled = true;

        world = AddComponent<Box2DWorldComponent>();

        world.SetDebugDraw(debugDrawCallback);

        if ((tilemap = TileMap.FromTiledMap(this, "content/maps/main.tmx")!) == null)
        {
            Engine.Logger.Log(
                Horizon.Logging.LogLevel.Fatal,
                "Failed to load tilemap, aborting..."
            );
            Environment.Exit(1);
        }
        else
            Entities.Add(tilemap);

        cam = new Camera(true) { Position = new Vector3(0, 0, 100) };

        InitializeRenderingPipeline();
    }

    public override void Initialize()
    {
        AddEntity(player = new Player2D(world, tilemap));

        spriteBatch = AddEntity(new SpriteBatch());
        spriteBatch.Add(player);

        AddEntity(
            particles = new ParticleRenderer2D(100_000, new BasicParticle2DMaterial())
            {
                MaxAge = 3.0f
            }
        );
        AddEntity(
            new IntervalRunner(
                1 / 25.0f,
                () =>
                {
                    var x = random.NextSingle() * Engine.Window.WindowSize.X;
                    var y = random.NextSingle() * Engine.Window.WindowSize.Y;

                    SpawnParticle(cam.ScreenToWorld(new Vector2(x, y)), playerDir);
                }
            )
        );

        base.Initialize();
    }

    protected override Effect[] GeneratePostProccessingEffects()
    {
        return new[] { new VingetteEffect() { Intensity = 1.0f } };
    }

    private void InitializeGl()
    {
        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        Engine.GL.Enable(EnableCap.Texture2D);
        Engine.GL.Enable(EnableCap.Blend);
        Engine.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private float cameraMovement = 16;
    private Vector2 playerDir;

    public override void UpdateState(float dt)
    {
        base.UpdateState(dt);

        playerDir = Engine.Input.GetVirtualController().MovementAxis;

        if (Engine.Input.KeyboardManager.IsKeyPressed(Key.F3))
            Engine.Debugger.Enabled = !Engine.Debugger.Enabled;

        cameraMovement +=
            15.0f
            * dt
            * (
                Engine.Input.KeyboardManager.IsKeyDown(Key.E)
                    ? -1
                    : Engine.Input.KeyboardManager.IsKeyDown(Key.Q)
                        ? 1
                        : 0
            );

        cam.Position = new Vector3(player.Position.X, player.Position.Y, cameraMovement);

        if (Engine.Input.KeyboardManager.IsKeyPressed(Key.G))
        {
            catCounter += 512;
        }
        if (
            Engine.Input.MouseManager
                .GetData()
                .Actions.HasFlag(Horizon.Input.VirtualAction.PrimaryAction)
        )
        {
            var mouseData = Engine.Input.MouseManager.GetData();
            SpawnParticle(
                cam.ScreenToWorld(mouseData.Position),
                new Vector2(1.0f - mouseData.Direction.X, mouseData.Direction.Y)
            );
        }

        cam.UpdateState(dt);

        //var mousePos = cam.ScreenToWorld(Engine.Input.MouseManager.GetData().Position);
        //for (int i = 0; i < tilemap.Depth; i++)
        //{
        //    if (tilemap[(int)mousePos.X, (int)mousePos.Y, i] is null)
        //        continue;
        //    tilemap[(int)mousePos.X, (int)mousePos.Y, i].RenderingData.Color = new Vector3(
        //        1.0f,
        //        0.0f,
        //        0.0f
        //    );
        //}
    }

    public override void Render(float dt, ref RenderOptions options)
    {
        charOptions = options with { Camera = cam };
        world.DrawDebugData();

        if (catCounter > 0)
        {
            Cat[] gattos = new Cat[catCounter];
            for (int i = 0; i < catCounter; i++)
            {
                var x = random.NextSingle() * Engine.Window.WindowSize.X;
                var y = random.NextSingle() * Engine.Window.WindowSize.Y;

                var cat = new Cat();
                cat.Transform.Position = cam.ScreenToWorld(new Vector2(x, y));

                gattos[i] = AddEntity(cat);
            }
            spriteBatch.AddRange(gattos);
            catCounter = 0;
        }

        debugDrawCallback.Enabled = options.IsBox2DDebugDrawEnabled;
        base.Render(dt, ref charOptions);
    }

    public override void DrawOther(float dt, ref RenderOptions options)
    {
        debugDrawCallback.Render(dt, ref options);
    }

    public override void Dispose()
    {
        debugDrawCallback.Dispose();
    }

    private void SpawnParticle(Vector2 pos, Vector2 dir)
    {
        float val = ((random.NextSingle() * 2.0f) - MathF.PI);

        particles.Add(
            new Particle2D(
                new Vector2(MathF.Sin(val), MathF.Cos(val)) * 0.5f + dir * 0.5f,
                pos,
                val
            )
        );
    }

    private void SpawnCircle(int count = 250)
    {
        float val = 0.0f;
        for (int i = 0; i < count; i++)
        {
            val = random.NextSingle() * MathF.PI * 2.0f;

            particles.Add(
                new Particle2D(
                    new Vector2(MathF.Cos(val), MathF.Sin(val)),
                    player.Position,
                    5.0f + random.NextSingle()
                )
            );
        }
    }

    public override void DrawGui(float dt)
    {
        if (ImGui.Begin("Particles (& cats)"))
        {
            ImGui.Text($"Particles Max: {particles.Count}");
            ImGui.Text($"Cats: {spriteBatch.Count - 1}");

            if (ImGui.Button("Spawn 100000"))
                SpawnCircle(100000);
            ImGui.End();
        }
    }
}
