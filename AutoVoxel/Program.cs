using System.Numerics;

using AutoVoxel.Data.Chunks;
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

    private const float MOVEMENT_SPEED = 5.0f;
    private const float GRAVITY = 7.0f;

    public override void Initialize()
    {
        base.Initialize();

        ActiveCamera = camera = AddEntity<Camera3D>();

        world = AddEntity<GameWorld>();
        camera.Position = new Vector3((world.ChunkManager.Width * Chunk.WIDTH) / 2.0f, 128, (world.ChunkManager.Height * Chunk.DEPTH) / 2.0f);

        MouseInputManager.Mouse.Cursor.CursorMode = Silk.NET.Input.CursorMode.Raw;
        Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
        Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.Texture2D);
        Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
    }

    public override void Render(float dt, object? obj = null)
    {
        Engine.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit | Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit);
        Engine.GL.Viewport(0, 0, (uint)Engine.WindowManager.ViewportSize.X, (uint)Engine.WindowManager.ViewportSize.Y);

        base.Render(dt, obj);
    }

    private bool isJumping = false;
    private float jumpTimer = 0.0f;
    public override void UpdateState(float dt)
    {
        float movementSpeed = MOVEMENT_SPEED * (Engine.InputManager.KeyboardManager.IsKeyDown(Silk.NET.Input.Key.ShiftLeft) ? 2.0f : 1.0f);
        Vector2 axis = Engine.InputManager.GetVirtualController().MovementAxis;
        Vector3 oldPos = camera.Position;
        Vector3 cameraFrontNoPitch = Vector3.Normalize(new Vector3(camera.Front.X, 0, camera.Front.Z));
        Vector3 movement = (Vector3.Normalize(Vector3.Cross(cameraFrontNoPitch, Vector3.UnitY)) * movementSpeed * axis.X * dt +
                            movementSpeed * cameraFrontNoPitch * axis.Y * dt) * new Vector3(1, 0, 1);
        Vector3 newPos = oldPos + movement;


        // Check collisions on x and z axes
        if ((int)world.ChunkManager[(int)newPos.X, (int)(newPos.Y - 1), (int)oldPos.Z].ID > 1 ||
            (int)world.ChunkManager[(int)newPos.X, (int)newPos.Y, (int)oldPos.Z].ID > 1)
        {
            newPos.X = oldPos.X;
        }

        if ((int)world.ChunkManager[(int)oldPos.X, (int)(newPos.Y - 1), (int)newPos.Z].ID > 1 ||
            (int)world.ChunkManager[(int)oldPos.X, (int)newPos.Y, (int)newPos.Z].ID > 1)
        {
            newPos.Z = oldPos.Z;
        }

        // Check collisions on the y axis (vertical collisions)
        if ((int)world.ChunkManager[(int)newPos.X, (int)newPos.Y - 1, (int)newPos.Z].ID > 1 ||
            (int)world.ChunkManager[(int)newPos.X, (int)(newPos.Y - 2), (int)newPos.Z].ID > 1)
        {
            newPos.Y = oldPos.Y;
        }

        camera.Position = newPos;


        // hacky gravity
        if ((int)world.ChunkManager[(int)camera.Position.X, (int)camera.Position.Y - 2, (int)camera.Position.Z].ID < 2)
        {
            camera.Position -= Vector3.UnitY * dt * GRAVITY;
        }
        else
        {
            if (!isJumping &&
                Engine.InputManager.GetVirtualController().IsPressed(VirtualAction.MoveJump))
            {
                isJumping = true;
            }
        }

        // even hackier jumping 
        JumpLogic(dt);


        if (float.IsNaN(camera.Position.X) || float.IsNaN(camera.Position.Y) || float.IsNaN(camera.Position.Z))
            camera.Position = new Vector3((world.ChunkManager.Width * Chunk.WIDTH) / 2.0f, 128, (world.ChunkManager.Height * Chunk.DEPTH) / 2.0f);

        base.UpdateState(dt);
    }

    private void JumpLogic(in float dt)
    {
        if (!isJumping) return;

        if (jumpTimer > 0.25f)
        {
            jumpTimer = 0.0f;
            isJumping = false;
            return;
        }


        jumpTimer += dt;
        camera.Position += Vector3.UnitY * dt * 15.0f;
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
