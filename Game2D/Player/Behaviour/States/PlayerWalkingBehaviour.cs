using Horizon.GameEntity;
using System.Numerics;

namespace TileBash.Player.Behaviour.States;

public class PlayerWalkingBehaviour : Player2DStateBehaviour
{
    public const float MOVEMENT_SPEED = 50.0f;

    public PlayerWalkingBehaviour(Player2DStateController controller)
        : base(controller, Player2DStateIdentifier.Walking) { }

    public override Player2DStateIdentifier Update(in float dt)
    {
        if (Entity.Engine.Input.GetVirtualController().MovementAxis.LengthSquared() == 0)
            return Player2DStateIdentifier.Idle;

        // Move player with input manager :).
        var movementDir = Entity.Engine.Input.GetVirtualController().MovementAxis;

        Player.PhysicsBody.ApplyForce(movementDir * MOVEMENT_SPEED, Player.Position);
        Player.PhysicsBody.SetLinearVelocity(
            Vector2.Clamp(Player.PhysicsBody.GetLinearVelocity(), Vector2.One * -5, Vector2.One * 5)
        );

        Player.FrameName = movementDir switch
        {
            var v when v.X < 0 => "walk_left",
            var v when v.X > 0 => "walk_right",
            var v when v.Y > 0 => "walk_up",
            var v when v.Y < 0 => "walk_down",
            _ => "idle"
        };

        return Player2DStateIdentifier.Walking;
    }
}
