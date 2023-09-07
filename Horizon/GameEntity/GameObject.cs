using Horizon.GameEntity.Components;
using Horizon.Rendering;
using System.Numerics;

namespace Horizon.GameEntity;

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
    public MeshRendererComponent MeshRenderer { get; private set; }

    /// <summary>
    /// The position of the game object in 3D space.
    /// </summary>
    public Vector3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    /// <summary>
    /// The scale of the game object in 3D space.
    /// </summary>
    public Vector3 Scale
    {
        get => Transform.Scale;
        set => Transform.Scale = value;
    }

    /// <summary>
    /// The rotation angles of the game object in degrees around each axis (X, Y, and Z).
    /// </summary>
    public Vector3 Rotation
    {
        get => Transform.Rotation;
        set => Transform.Rotation = value;
    }

    /// <summary>
    /// The material used for rendering the game object.
    /// </summary>
    public Material Material
    {
        get => MeshRenderer.Material;
        set => MeshRenderer.Material = value;
    }

    /// <summary>
    /// Creates a new instance of the GameObject class.
    /// </summary>
    public GameObject(Material? material = null, MeshData? meshData = null)
    {
        // Add a transform component to the game object.
        Transform = AddComponent<TransformComponent>();

        // Add a mesh renderer component to the game object.
        MeshRenderer = AddComponent<MeshRendererComponent>();

        if (meshData.HasValue)
            MeshRenderer.Load(meshData.Value, material);
        else if (material != null)
            Material = material;
    }
}
