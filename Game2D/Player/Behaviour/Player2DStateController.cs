using Horizon;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;

namespace Game2D.Player.Behaviour;

public class Player2DStateController : IGameComponent
{
    public Dictionary<Player2DStateIdentifier, Player2DStateBehaviour> StateBehaviours { get; init; }
    public string Name { get; set; } = "Player2D State Controller";
    public Entity Parent { get; set; }
    public Player2D Player { get; private set; }

    public Player2DStateIdentifier CurrentState { get; protected set; }

    public Player2DStateController()
    {
        this.StateBehaviours = new();

        CurrentState = Player2DStateIdentifier.Idle;
    }

    public void RegisterBehaviour(Player2DStateIdentifier identifier, Player2DStateBehaviour behaviour)
    {
        if (!StateBehaviours.TryAdd(identifier, behaviour))
            Entity.Engine.Logger.Log(Horizon.Logging.LogLevel.Error, $"[State Controller] Failed to register state {identifier}!");
    }

    public void Initialize() 
    {
        Player = (Player2D)Parent;
    }

    public void Update(float dt)
    {
        CurrentState = StateBehaviours[CurrentState].Update(dt);
    }

    public void Draw(float dt, RenderOptions? options = null)
    {
        
    }
}
