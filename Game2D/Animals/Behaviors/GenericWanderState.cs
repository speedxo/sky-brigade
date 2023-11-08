using System.Numerics;

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
        // Random wander time between 0 and 9.9 seconds
        _targetWanderTime = Random.NextSingle() * 10.0f;
        _wanderingTimer = 0;

        // Random direction
        float value = Random.NextSingle() * 2.0f * MathF.PI;
        _targetDir = new Vector2(MathF.Cos(value), MathF.Sin(value) * 0.25f);
    }

    public override void Exit() { }

    public override void Update(float dt)
    {
        // Set animation
        Parent.SetAnimation("run");

        Parent.Flipped = _targetDir.X < 0;

        _wanderingTimer += dt;
        Parent.Transform.Position += _targetDir * dt * Speed;

        if (_wanderingTimer > _targetWanderTime)
            StateMachine.Transition(AnimalBehavior.Idle);
    }
}
