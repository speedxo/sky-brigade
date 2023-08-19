using Silk.NET.Input;
using System.Numerics;

namespace Horizon.Rendering;

public class Camera
{
    //Setup the camera's location, directions, and movement speed
    protected Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);

    protected Vector3 CameraUp = Vector3.UnitY;
    protected Vector3 _camDir = Vector3.Zero;
    protected float CameraZoom = 45f;

    public Vector3 Rotation { get => _camDir; set => _camDir = value; }

    public Matrix4x4 View { get; protected set; }
    public Matrix4x4 Projection { get; protected set; }
    public Vector3 Position { get; set; } = new Vector3(0, 0, 10);

    public bool Locked { get; set; } = false;

    public Camera()
    {
        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)GameManager.Instance.ViewportSize.X / (float)GameManager.Instance.ViewportSize.Y, 0.1f, 100.0f);
    }

    public virtual void Update(float dt)
    {
        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)GameManager.Instance.ViewportSize.X / (float)GameManager.Instance.ViewportSize.Y, 0.1f, 100.0f);
    }
    public bool IsPointInFrustum(Vector2 point) => IsPointInFrustum(new Vector3(point, 0.0f));

    public bool IsPointInFrustum(Vector3 point)
    {
        return true;

        Matrix4x4 viewProjection = View * Projection;

        // Calculate the transformed point in homogeneous coordinates
        Vector4 transformedPoint = Vector4.Transform(new Vector4(point, 1.0f), viewProjection);

        // Check if the transformed point is within the normalized device coordinates
        bool insideFrustum =
            transformedPoint.X >= -transformedPoint.W &&
            transformedPoint.X <= transformedPoint.W &&
            transformedPoint.Y >= -transformedPoint.W &&
            transformedPoint.Y <= transformedPoint.W &&
            transformedPoint.Z >= -transformedPoint.W &&
            transformedPoint.Z <= transformedPoint.W;

        return insideFrustum;
    }
}