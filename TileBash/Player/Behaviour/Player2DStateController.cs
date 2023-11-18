using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Rendering;

namespace TileBash.Player.Behaviour;

public class Player2DStateController : IGameComponent
{
    public Dictionary<
        Player2DStateIdentifier,
        Player2DStateBehaviour
    > StateBehaviours { get; init; }
    public string Name { get; set; } = "Player2D State Controller";
    public Entity Parent { get; set; }
    public Player2D Player { get; private set; }

    public Player2DStateIdentifier CurrentState { get; protected set; }
    public bool Enabled { get; set; }

    public Player2DStateController(in Player2D player)
    {
        this.Player = player;
        this.StateBehaviours = new();

        CurrentState = Player2DStateIdentifier.Idle;
    }

    public void RegisterBehaviour(
        Player2DStateIdentifier identifier,
        Player2DStateBehaviour behaviour
    )
    {
        if (!StateBehaviours.TryAdd(identifier, behaviour))
        {
            //Entity.ConcurrentLogger.Instance.Log(
            //    Horizon.Logging.LogLevel.Error,
            //    $"[State Controller] Failed to register state {identifier}!"
            //); // TODO: fix
        }
    }

    public void Initialize()
    {
        Player = (Player2D)Parent;
    }

    public void UpdateState(float dt)
    {
        CurrentState = StateBehaviours[CurrentState].Update(dt);
    }

    public void UpdatePhysics(float dt) { }

    public void Render(float dt, object? obj = null) { }
}
