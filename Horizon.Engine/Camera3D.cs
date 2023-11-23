using System.Numerics;
using Box2D.NetStandard.Common;
using Horizon.Core;

namespace Horizon.Engine;

public class Camera3D : Camera
{
    private float lookSensitivity = 0.1f;
    private float CameraYaw = 0f;
    private float CameraPitch = 0f;

    public Vector3 Rotation { get; private set; }
    public Vector3 Front { get; private set; }

    public Camera3D()
        : this(90.0f) { }

    public Camera3D(in float fov = 45)
    {
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(fov),
            GameEngine.Instance.WindowManager.AspectRatio,
            0.1f,
            100.0f
        );
    }

    protected override void UpdateMatrices()
    {
        View = Matrix4x4.CreateLookAt(Position, Position + Front, CameraUp);
        ProjView = View * Projection;
    }

    private void UpdateMouse()
    {
        var controller = Engine.InputManager.GetVirtualController();

        var xOffset = (controller.LookingAxis.X) * lookSensitivity;
        var yOffset = (controller.LookingAxis.Y) * lookSensitivity;

        CameraYaw -= xOffset;
        CameraPitch += yOffset;

        // We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
        CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

        Rotation = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(CameraYaw))
                * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch)),
            MathF.Sin(MathHelper.DegreesToRadians(CameraPitch)),
            MathF.Sin(MathHelper.DegreesToRadians(CameraYaw))
                * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch))
        );

        Front = Vector3.Normalize(Rotation);
    }

    public override void UpdateState(float dt)
    {
        base.UpdateState(dt);
        UpdateMouse();
    }
}
