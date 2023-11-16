using Horizon.Core.Primitives;

namespace TileBash.Animals.Behaviors;

internal abstract class AnimalState : IUpdateable
{
    protected static Random Random { get; }

    static AnimalState()
    {
        Random = new Random(Environment.TickCount);
    }

    public AnimalBehaviorStateMachineComponent StateMachine { get; init; }
    public Animal Parent { get; init; }

    public AnimalState(Animal parent, AnimalBehaviorStateMachineComponent stateMachine)
    {
        this.Parent = parent;
        this.StateMachine = stateMachine;
    }

    public abstract void UpdatePhysics(float dt);
    public abstract void UpdateState(float dt);
    public abstract void Enter();
    public abstract void Exit();
}
