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
    public bool Use2D { get; }

    public Camera(bool is2D)
    {
        this.Use2D = is2D;
        CalculateMatricesAndUpdateBounds(Engine.Window.AspectRatio);
    }

    public Camera()
        : this(false) { }

    /// <summary>
    /// Projects a screen space position to world space.
    /// </summary>
    /// <param name="screenPosition">The screen position.</param>
    /// <returns></returns>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        // Normalize the screen position from [0, 1] to [-1, 1]
        Vector2 normalizedScreenPosition = new Vector2(
            (screenPosition.X / Engine.Window.WindowSize.X) * 2.0f - 1.0f,
            1.0f - (screenPosition.Y / Engine.Window.WindowSize.Y) * 2.0f
        );

        // Calculate the inverse view-projection matrix
        Matrix4x4 inverseViewProj;
        if (Matrix4x4.Invert(viewProj, out inverseViewProj))
        {
            // Transform the normalized screen position into world coordinates
            Vector4 worldPosition4D = Vector4.Transform(
                new Vector4(normalizedScreenPosition, 0.0f, 1.0f),
                inverseViewProj
            );
            Vector3 worldPosition = new Vector3(
                worldPosition4D.X,
                worldPosition4D.Y,
                worldPosition4D.Z
            );

            return new Vector2(worldPosition.X, worldPosition.Y);
        }

        // Return a default value if the inverse matrix is not valid
        return Vector2.Zero;
    }

    protected virtual void CalculateMatricesAndUpdateBounds(float aspectRatio)
    {
        View = Matrix4x4.CreateLookAt(Position, Position + CameraFront, CameraUp);

        if (Use2D)
        {
            Projection = Matrix4x4.CreateOrthographic(
                aspectRatio * Position.Z,
                Position.Z,
                1.0f,
                1000.0f
            );

            var h = Position.Z;
            var w = h * aspectRatio;

            Bounds = new RectangleF(Position.X - w / 2.0f, Position.Y - h / 2.0f, w, h);
        }
        else
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(CameraZoom),
                aspectRatio,
                1.0f,
                1000.0f
            );

            var h = MathF.Tan(CameraZoom / 2.0f) * Position.Z * 1.5f;
            var w = h * aspectRatio;

            Bounds = new RectangleF(Position.X - w / 2.0f, Position.Y - h / 2.0f, w, h);
        }

        viewProj = View * Projection;
    }

    public override void UpdateState(float dt)
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
