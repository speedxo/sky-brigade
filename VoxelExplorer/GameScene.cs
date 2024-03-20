using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Box2D.NetStandard.Dynamics.World;

using Horizon.Engine;
using Horizon.Input.Components;
using Horizon.Input;

using VoxelExplorer.Data;

namespace VoxelExplorer
{
    internal class GameScene : Scene
    {
        private VoxelWorld world;
        private VoxelWorldRenderer renderer;
        private Camera3D camera;

        public override Camera ActiveCamera { get; protected set; }

        private const float MOVEMENT_SPEED = 5.0f;
        private bool captureInput = true;

        public override void Initialize()
        {
            base.Initialize();

            world = new VoxelWorld();
            renderer = AddEntity(new VoxelWorldRenderer(world));
            ActiveCamera = camera = AddEntity<Camera3D>();


            Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
            Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.Texture2D);
            Engine.GL.Enable(Silk.NET.OpenGL.EnableCap.DepthTest);
        }

        public override void UpdateState(float dt)
        {
            base.UpdateState(dt);

            if (Engine.InputManager.WasPressed(VirtualAction.Pause))
            {
                captureInput = !captureInput;
                MouseInputManager.Mouse.Cursor.CursorMode = captureInput ? Silk.NET.Input.CursorMode.Raw : Silk.NET.Input.CursorMode.Normal;
            }

            if (!captureInput) return;

            float movementSpeed = MOVEMENT_SPEED * (Engine.InputManager.KeyboardManager.IsKeyDown(Silk.NET.Input.Key.ShiftLeft) ? 2.0f : 1.0f);
            Vector2 axis = Engine.InputManager.GetVirtualController().MovementAxis;
            Vector3 oldPos = camera.Position;
            Vector3 cameraFrontNoPitch = Vector3.Normalize(new Vector3(camera.Front.X, 0, camera.Front.Z));
            Vector3 movement = (Vector3.Normalize(Vector3.Cross(camera.Front, Vector3.UnitY)) * movementSpeed * axis.X * dt +
                                movementSpeed * camera.Front * axis.Y * dt) * new Vector3(1, 1, 1);
            camera.Position = oldPos + movement;
        }

        public override void Render(float dt, object? obj = null)
        {
            Engine.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit | Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit);
            Engine.GL.Viewport(0, 0, (uint)Engine.WindowManager.ViewportSize.X, (uint)Engine.WindowManager.ViewportSize.Y);

            base.Render(dt, obj);
        }
    }
}
