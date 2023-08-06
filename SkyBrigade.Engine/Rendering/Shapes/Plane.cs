using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using System.Numerics;

namespace SkyBrigade.Engine.Rendering.Shapes;

/// <summary>
/// Represents a plane entity in the game world.
/// </summary>
public class Plane : Entity
{
    /// <summary>
    /// The transform component that handles the transformation of the plane entity.
    /// </summary>
    public TransformComponent Transform { get; protected set; }

    /// <summary>
    /// The mesh renderer component responsible for rendering the plane.
    /// </summary>
    public MeshRendererComponent MeshRenderer { get; protected set; }

    /// <summary>
    /// The position of the plane in 3D space.
    /// </summary>
    public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }

    /// <summary>
    /// The rotation angles of the plane in degrees around each axis (X, Y, and Z).
    /// </summary>
    public Vector3 Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }

    /// <summary>
    /// The material used for rendering the plane.
    /// </summary>
    public Material Material { get => MeshRenderer.Material; set => MeshRenderer.Material = value; }

    /// <summary>
    /// Creates a new instance of the Plane class.
    /// </summary>
    /// <param name="material">The material to use for rendering the plane. If null, the default material will be used.</param>
    public Plane(Material? material = null)
    {
        // Add a transform component to the plane entity.
        Transform = AddComponent<TransformComponent>();

        // Add a mesh renderer component to the plane entity and load a rectangle mesh.
        MeshRenderer = AddComponent<MeshRendererComponent>();
        MeshRenderer.Load(MeshGenerators.CreateRectangle, material);
    }
}
