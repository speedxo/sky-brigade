namespace TileBash.Player.Behaviour;

public abstract class Player2DStateBehaviour
{
    public Player2DStateIdentifier StateIdentifier { get; init; }
    protected Player2DStateController Controller { get; init; }
    protected Player2D Player { get; init; }

    public Player2DStateBehaviour(
        Player2DStateController controller,
        Player2DStateIdentifier identifier
    )
    {
        this.StateIdentifier = identifier;
        this.Controller = controller;
        this.Player = controller.Player;
    }

    public abstract Player2DStateIdentifier Update(in float dt);
}
