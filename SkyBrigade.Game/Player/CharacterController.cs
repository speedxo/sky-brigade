using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Input;
using System.Numerics;

namespace SkyBrigade.Game.Player
{
    public partial class CharacterController : Entity
    {
        public TransformComponent Transform { get; private set; }
        public CameraComponent Camera { get; private set; }
        public CharacterMovementController MovementController { get; private set; }

        // how do position and rotation have the same number of letters?
        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public Vector3 Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }

        public CharacterController()
        {
            Transform = AddComponent<TransformComponent>();
            Camera = AddComponent<CameraComponent>();
            MovementController = AddComponent<CharacterMovementController>();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}