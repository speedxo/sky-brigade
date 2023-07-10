using System;
using System.Numerics;
using System.Reflection;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using SkyBrigade.Engine;

namespace SkyBrigade.Engine.Rendering;

public class Camera
{
    //Setup the camera's location, directions, and movement speed
    private Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 CameraUp = Vector3.UnitY;
    private Vector3 _camDir = Vector3.Zero;
    private float CameraYaw = -90f;
    private float CameraPitch = 0f;
    private float CameraZoom = 45f;

    public Vector3 CameraDirection { get => _camDir; }

    //Used to track change in mouse movement to allow for moving of the Camera
    private System.Numerics.Vector2 LastMousePosition;

    public Matrix4x4 View { get; private set; }
    public Matrix4x4 Projection { get; private set; }
    public Vector3 Position { get; set; } = new Vector3(0, 0, 10);

    public bool Locked { get; set; } = false;
    
    public Camera()
    {
        // this is hacky as shit balls 
        for (int i = 0; i < GameManager.Instance.Input.Mice.Count; i++)
        {
            GameManager.Instance.Input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
            GameManager.Instance.Input.Mice[i].MouseMove += OnMouseMove;
            GameManager.Instance.Input.Mice[i].Scroll += OnMouseWheel;
        }

        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)GameManager.Instance.Window.FramebufferSize.X / (float)GameManager.Instance.Window.FramebufferSize.Y, 0.01f, 100.0f);
    }


    public void Update(float dt)
    {
        if (!Locked && GameManager.Instance.IsInputCaptured)
        {
            var moveSpeed = 18f * dt;

            if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Key.W))
            {
                //Move forwards
                Position += moveSpeed * CameraFront;
            }
            if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Key.S))
            {
                //Move backwards
                Position -= moveSpeed * CameraFront;
            }
            if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Key.A))
            {
                //Move left
                Position -= Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
            }
            if (GameManager.Instance.Input.Keyboards[0].IsKeyPressed(Key.D))
            {
                //Move right
                Position += Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
            }
        }

        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)GameManager.Instance.Window.FramebufferSize.X / (float)GameManager.Instance.Window.FramebufferSize.Y, 0.1f, 100.0f);
    }

    private unsafe void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        if (!GameManager.Instance.IsInputCaptured || Locked) return;

        var lookSensitivity = 0.1f;
        if (LastMousePosition == default) { LastMousePosition = position; }
        else
        {
            var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
            var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
            LastMousePosition = position;

            CameraYaw += xOffset;
            CameraPitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

            _camDir.X = MathF.Cos(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
            _camDir.Y = MathF.Sin(MathHelper.DegreesToRadians(CameraPitch));
            _camDir.Z = MathF.Sin(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch));
            CameraFront = Vector3.Normalize(_camDir);
        }
    }

    private unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        if (!GameManager.Instance.IsInputCaptured || Locked) return;

        //We don't want to be able to zoom in too close or too far away so clamp to these values
        CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
    }
}

