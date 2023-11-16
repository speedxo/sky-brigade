namespace TileBash.Player.Behaviour.States;

public class PlayerIdleBehaviour : Player2DStateBehaviour
{
    public PlayerIdleBehaviour(Player2DStateController controller)
        : base(controller, Player2DStateIdentifier.Idle) { }

    public override Player2DStateIdentifier Update(in float dt)
    {
        if (Engine.Input.GetVirtualController().MovementAxis.LengthSquared() > 0)
            return Player2DStateIdentifier.Walking;

        Player.SetAnimation("idle");

        return Player2DStateIdentifier.Idle;
    }
}
