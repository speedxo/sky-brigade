using System.Drawing;
using System.Numerics;

namespace Horizon.Engine;

public class Camera2D : Camera
{
    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            Projection = Matrix4x4.CreateOrthographic(Size.X * Zoom, Size.Y * Zoom, 0.1f, 1.0f);
        }
    }

    private float _zoom = 1.0f;
    private Vector2 Size { get; set; }

    public Camera2D(in Vector2 size)
    {
        this.Size = size;
        Zoom = 1.0f;
    }

    protected override void UpdateMatrices()
    {
        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        ProjView = View * Projection;

        Bounds = new RectangleF(
            Position.X - 0.5f * Size.X * Zoom,
            Position.Y - 0.5f * Size.Y * Zoom,
            Zoom * Size.X,
            Zoom * Size.Y
        );
    }
}
