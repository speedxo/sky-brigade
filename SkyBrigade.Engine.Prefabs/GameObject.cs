using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.Prefabs;

/// <summary>
/// Represents a game object in the game world with custom behavior.
/// </summary>
public class GameObject : Entity
{
    /// <summary>
    /// The transform component that handles the transformation of the game object.
    /// </summary>
    public TransformComponent Transform { get; private set; }

    /// <summary>
    /// The mesh renderer component responsible for rendering the game object.
    /// </summary>
    public MeshRendererComponent Mesh { get; private set; }

    /// <summary>
    /// The custom behavior component responsible for handling custom logic.
    /// </summary>
    public CustomBehaviourComponent CustomBehaviour { get; private set; }

    /// <summary>
    /// The position of the game object in 3D space.
    /// </summary>
    public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }

    /// <summary>
    /// The rotation angles of the game object in degrees around each axis (X, Y, and Z).
    /// </summary>
    public Vector3 Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }

    /// <summary>
    /// The material used for rendering the game object.
    /// </summary>
    public Material Material { get => Mesh.Material; set => Mesh.Material = value; }

    /// <summary>
    /// Creates a new instance of the GameObject class.
    /// </summary>
    public GameObject()
    {
        // Add a transform component to the game object.
        Transform = AddComponent<TransformComponent>();

        // Add a mesh renderer component to the game object.
        Mesh = AddComponent<MeshRendererComponent>();

        // Add a custom behavior component to the game object.
        CustomBehaviour = AddComponent<CustomBehaviourComponent>();
    }
}
