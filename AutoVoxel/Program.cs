﻿using System.Numerics;

using AutoVoxel.World;

using Box2D.NetStandard.Dynamics.World;

using Horizon.Engine;
using Horizon.Input;
using Horizon.Input.Components;
using Horizon.Rendering;
using Horizon.Rendering.Techniques;

namespace AutoVoxel;

internal class Program : Scene
{
    public override Camera ActiveCamera { get; protected set; }
    private Camera3D camera;
    private GameWorld world;

    public override void Initialize()
    {
        base.Initialize();

        ActiveCamera = camera = AddEntity<Camera3D>();

        world = AddEntity<GameWorld>();


        MouseInputManager.Mouse.Cursor.CursorMode = Silk.NET.Input.CursorMode.Raw;
        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.Texture2D);
        Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
    }

    public override void Render(float dt, object? obj = null)
    {
        Engine.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit | Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit);

        base.Render(dt, obj);
    }

    public override void UpdateState(float dt)
    {
        var axis = Engine.InputManager.GetVirtualController().MovementAxis;

        camera.Position +=
                 Vector3.Normalize(Vector3.Cross(camera.Front, Vector3.UnitY))
                 * 5
                 * axis.X
                 * dt;
        camera.Position += 5 * camera.Front * axis.Y * dt;
        if (float.IsNaN(camera.Position.X) || float.IsNaN(camera.Position.Y) || float.IsNaN(camera.Position.Z))
            camera.Position = new Vector3(0, 0, 0);

        base.UpdateState(dt);
    }

    static void Main(string[] _)
    {
        var engine = new GameEngine(
            GameEngineConfiguration.Default with
            {
                InitialScene = typeof(Program)
            }
        );
        engine.Run();
    }
}