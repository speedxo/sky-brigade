using Horizon;

namespace Game2D.Player.Behaviour.States;

public class PlayerWalkingBehaviour : Player2DStateBehaviour
{
    public PlayerWalkingBehaviour(Player2DStateController controller) 
        : base(controller, Player2DStateIdentifier.Walking)
    {
    }

    public override Player2DStateIdentifier Update(in float dt)
    {
        if (GameManager.Instance.InputManager.GetVirtualController().MovementAxis.LengthSquared() == 0)
            return Player2DStateIdentifier.Idle;

        return Player2DStateIdentifier.Walking;
    }
}
