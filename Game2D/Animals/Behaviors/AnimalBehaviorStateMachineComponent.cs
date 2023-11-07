using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;

namespace TileBash.Animals.Behaviors;

internal class AnimalBehaviorStateMachineComponent : IGameComponent
{
    public AnimalState CurrentState { get; protected set; }
    public Dictionary<AnimalBehavior, AnimalState> States { get; init; } = new();

    public string Name { get; set; } = "Animal Behavior State Machine";
    public Entity Parent { get; set; }

    public void Initialize() { }

    public void AddState(AnimalBehavior behavior, AnimalState state)
    {
        States.Add(behavior, state);
        CurrentState ??= state;
    }

    public void Transition(AnimalBehavior behavior)
    {
        CurrentState?.Exit();
        CurrentState = States[behavior];
        CurrentState.Enter();
    }

    public void Update(float dt)
    {
        CurrentState?.Update(dt);
    }

    public void Draw(float dt, ref RenderOptions options) { }
}
