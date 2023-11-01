using Horizon.Data;
using Horizon.GameEntity;
using System.Numerics;

namespace Horizon.Rendering.Shapes;

/// <summary>
/// Represents a plane entity in the game world.
/// </summary>
public class Plane : GameObject
{
    /// <summary>
    /// The size of the plane in 3D space.
    /// </summary>
    public Vector2 Size
    {
        get => size;
        set
        {
            size = value;
            UpdateVertices();
        }
    }

    private Vector2 size = Vector2.One;

    /// <summary>
    /// Creates a new instance of the Plane class.
    /// </summary>
    /// <param name="material">The material to use for rendering the plane. If null, the default material will be used.</param>
    public Plane(
        Material? material = null,
        Vector3? initialScale = null,
        Vector3? initialPosition = null,
        Vector2? initialSize = null
    )
        : base(material)
    {
        Transform.Scale = initialScale ?? Vector3.One;
        Transform.Position = initialPosition ?? default;

        MeshRenderer.Load(MeshGenerators.CreateRectangle, material);
        Size = initialSize ?? Vector2.One;
    }

    /// <summary>
    /// Helper method so that the plane can update its vertices after they are changed.
    /// </summary>
    private void UpdateVertices()
    {
        MeshRenderer.Buffer.VertexBuffer.BufferData(
            new Vertex[]
            {
                new Vertex(-Size.X, -Size.Y, 0, 0, 1),
                new Vertex(Size.X, -Size.Y, 0, 1, 1),
                new Vertex(Size.X, Size.Y, 0, 1, 0),
                new Vertex(-Size.X, Size.Y, 0, 0, 0)
            }
        );
    }

    /// <summary>
    /// Returns true if this plane intersects with plane2.
    /// </summary>
    /// <param name="plane2">The plane to check intersection with.</param>
    /// <returns>True if the planes intersect</returns>
    public bool CheckIntersection(Plane plane2)
    {
        // Get the corner points of each rectangle
        Vector2 rect1TopLeft = new Vector2(Position.X - Size.X, Position.Y + Size.Y);
        Vector2 rect1BottomRight = new Vector2(Position.X + Size.X, Position.Y - Size.Y);

        Vector2 rect2TopLeft = new Vector2(
            plane2.Position.X - plane2.Size.X,
            plane2.Position.Y + plane2.Size.Y
        );
        Vector2 rect2BottomRight = new Vector2(
            plane2.Position.X + plane2.Size.X,
            plane2.Position.Y - plane2.Size.Y
        );

        // Check for intersection
        if (
            rect1TopLeft.X <= rect2BottomRight.X
            && rect1BottomRight.X >= rect2TopLeft.X
            && rect1TopLeft.Y >= rect2BottomRight.Y
            && rect1BottomRight.Y <= rect2TopLeft.Y
        )
        {
            // Rectangles intersect
            return true;
        }

        // Rectangles do not intersect
        return false;
    }
}
