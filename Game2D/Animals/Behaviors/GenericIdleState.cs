using System.Numerics;
using TileBash.Player;

namespace TileBash.Animals.Behaviors;

internal class GenericIdleState : AnimalState
{
    public GenericIdleState(Animal parent, AnimalBehaviorStateMachineComponent stateMachine)
        : base(parent, stateMachine) { }

    private float _targetIdleTime,
        _idleTimer;

    public override void Enter()
    {
        // Set animation
        Parent.SetAnimation("idle");

        // Idle for a random amount
        _targetIdleTime = Random.NextSingle() * 60.0f;
        _idleTimer = 0.0f;

        // Incase player suddenly approaches
        UpdateState(0.0f);
    }

    public override void Exit() { }

    public override void UpdateState(float dt)
    {
        if (Vector2.DistanceSquared(Player2D.Current.Position, Parent.Transform.Position) < 25.0f)
            StateMachine.Transition(AnimalBehavior.Wander);

        _idleTimer += dt;
        if (_idleTimer > _targetIdleTime)
            StateMachine.Transition(AnimalBehavior.Wander);
    }

    public override void UpdatePhysics(float dt) { }
}
