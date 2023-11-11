using Horizon.GameEntity;
using System.Numerics;
using TileBash.Player;

namespace TileBash.Animals.Behaviors;

internal class GenericWanderState : AnimalState
{
    public float Speed { get; set; } = 2.0f;

    public GenericWanderState(Animal parent, AnimalBehaviorStateMachineComponent stateMachine)
        : base(parent, stateMachine) { }

    private Vector2 _targetDir;
    private float _targetWanderTime = 0.0f,
        _wanderingTimer;

    public override void Enter()
    {
        // Set animation
        Parent.SetAnimation("run");

        // Random wander time between 0 and 9.9 seconds
        _targetWanderTime = Random.NextSingle() * 10.0f;
        _wanderingTimer = 0;

        // Random direction
        float value = Random.NextSingle() * 2.0f * MathF.PI;
        _targetDir =
            new Vector2(MathF.Cos(value), MathF.Sin(value) * 0.25f) * 0.25f
            + Entity.Engine.Input.GetVirtualController().MovementAxis * 0.75f;
    }

    public override void Exit() { }

    public override void UpdateState(float dt)
    {
        Parent.Flipped = _targetDir.X < 0;

        _wanderingTimer += dt;
        Parent.Transform.Position += _targetDir * dt * Speed;

        if (_wanderingTimer > _targetWanderTime)
            StateMachine.Transition(AnimalBehavior.Idle);
    }

    public override void UpdatePhysics(float dt) { }
}
