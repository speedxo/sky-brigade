using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System.Numerics;

namespace Horizon.Prefabs.Character;

public partial class CharacterController : Entity
{
    public TransformComponent Transform { get; init; }
    public CameraComponent Camera { get; init; }
    public CharacterMovementController MovementController { get; init; }

    // how do position and rotation have the same number of letters?
    public Vector3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    public Vector3 Rotation
    {
        get => Transform.Rotation;
        set => Transform.Rotation = value;
    }

    public CharacterController(CharacterMovementControllerConfig config)
    {
        Transform = AddComponent<TransformComponent>();
        Camera = AddComponent<CameraComponent>();
        MovementController = AddComponent(new CharacterMovementController(config));
    }

    public override void Update(float dt)
    {
        base.Update(dt);
    }
}
