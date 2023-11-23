using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Box2D.NetStandard.Common;
using Horizon.Core.Components;

namespace Horizon.Engine;

public abstract class Camera : GameObject
{
    protected Vector3 CameraFront = -Vector3.UnitZ;
    protected Vector3 CameraUp = Vector3.UnitY;

    public Matrix4x4 View { get; protected set; }
    public Matrix4x4 Projection { get; protected set; }
    public Matrix4x4 ProjView { get; protected set; }

    public RectangleF Bounds { get; protected set; }
    public Vector3 Position { get; set; }

    public override void Render(float dt, object? obj = null)
    {
        UpdateMatrices();
        ProjView = View * Projection;
        base.Render(dt);
    }

    protected abstract void UpdateMatrices();

    /// <summary>
    /// Projects a screen space position to world space.
    /// </summary>
    /// <param name="screenPosition">The screen position.</param>
    /// <returns></returns>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        // Normalize the screen position from [0, 1] to [-1, 1]
        Vector2 normalizedScreenPosition = new Vector2(
            (screenPosition.X / Engine.WindowManager.WindowSize.X) * 2.0f - 1.0f,
            1.0f - (screenPosition.Y / Engine.WindowManager.WindowSize.Y) * 2.0f
        );

        // Calculate the inverse view-projection matrix
        Matrix4x4 inverseViewProj;
        if (Matrix4x4.Invert(ProjView, out inverseViewProj))
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

    /// <summary>
    /// Projects a screen space position to world space.
    /// </summary>
    /// <param name="screenPosition">The screen position.</param>
    /// <returns></returns>
    public Vector2 WorldToScreen(Vector2 screenPosition)
    {
        // Calculate the inverse view-projection matrix

        // Transform the normalized screen position into world coordinates
        Vector4 worldPosition4D = Vector4.Transform(
            new Vector4(screenPosition, 0.0f, 1.0f),
            ProjView
        );
        Vector3 worldPosition = new Vector3(
            worldPosition4D.X,
            worldPosition4D.Y,
            worldPosition4D.Z
        );

        return new Vector2(worldPosition.X, worldPosition.Y);
    }
}
