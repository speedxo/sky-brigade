using Horizon.GameEntity;
using System.Numerics;

namespace Horizon.Rendering;

public class Camera : Entity
{
    //Setup the camera's location, directions, and movement speed
    protected Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);

    protected Vector3 CameraUp = Vector3.UnitY;
    protected Vector3 _camDir = Vector3.Zero;
    protected float CameraZoom = 45f;
    protected Matrix4x4 viewProj;

    public Vector3 Rotation
    {
        get => _camDir;
        set => _camDir = value;
    }

    public Matrix4x4 View { get; protected set; }
    public Matrix4x4 Projection { get; protected set; }
    public Vector3 Position { get; set; } = new Vector3(0, 0, 10);
    public RectangleF Bounds { get; protected set; }

    public bool Locked { get; set; } = false;

    public Camera()
    {
        CalculateMatricesAndUpdateBounds(Engine.Window.AspectRatio);
    }

    protected virtual void CalculateMatricesAndUpdateBounds(float aspectRatio)
    {
        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(CameraZoom),
            aspectRatio,
            1.0f,
            1000.0f
        );

        viewProj = View * Projection;

        var h = MathF.Tan(CameraZoom / 2.0f) * Position.Z * 1.5f;
        var w = h * aspectRatio;

        Bounds = new RectangleF(Position.X - w / 2.0f, Position.Y - h / 2.0f, w, h);
    }

    public override void Update(float dt)
    {
        CalculateMatricesAndUpdateBounds(Engine.Window.AspectRatio);
    }

    public bool IsPointInFrustum(Vector2 point) => IsPointInFrustum(new Vector3(point, 0.0f));

    public bool IsPointInFrustum(Vector3 point)
    {
        // Calculate the transformed point in homogeneous coordinates
        Vector4 transformedPoint = Vector4.Transform(new Vector4(point, 1.0f), viewProj);

        // Check if the transformed point is within the normalized device coordinates
        bool insideFrustum =
            transformedPoint.X >= -transformedPoint.W
            && transformedPoint.X <= transformedPoint.W
            && transformedPoint.Y >= -transformedPoint.W
            && transformedPoint.Y <= transformedPoint.W
            && transformedPoint.Z >= -transformedPoint.W
            && transformedPoint.Z <= transformedPoint.W;

        return insideFrustum;
    }
}
