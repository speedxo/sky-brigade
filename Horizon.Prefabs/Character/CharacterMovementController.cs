using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using System.Numerics;

namespace Horizon.Prefabs.Character;

public partial class CharacterController
{
    [RequiresComponent(typeof(TransformComponent))]
    public class CharacterMovementController : IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }
        public CharacterController Controller { get; private set; }
        public TransformComponent Transform { get; private set; }
        public CharacterMovementControllerConfig Config { get; private set; }

        public Vector3 Position
        {
            get => Transform.Position;
            set => Transform.Position = value;
        }
        public Vector3 Rotation
        {
            get => Transform.Rotation;
            set => Transform.Rotation = value;
        }
        public Vector3 Front
        {
            get => Transform.Front;
        }

        public CharacterMovementController()
        {
            Name = "Character Movement controller";
        }

        public void Initialize()
        {
            Config = CharacterMovementControllerConfig.Default;

            Controller = (CharacterController)Parent;

            Transform = Parent.GetComponent<TransformComponent>()!;
            Transform.Position = new Vector3(0, 5, -2);
        }

        public void Update(float dt)
        {
            DoLocomotion(dt);
        }

        private void DoLocomotion(float dt)
        {
            var moveSpeed = Config.BaseMovementSpeed * dt;

            var virtualController = Engine.Input.GetVirtualController();

            Position +=
                Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY))
                * moveSpeed
                * virtualController.MovementAxis.X;
            Position += moveSpeed * Front * virtualController.MovementAxis.Y;

            // there apears to be some funky wunky happenings with the underlying
            // glfw window wrapper silk.net is using sooooooooooooo
            if (float.IsNaN(Position.X) || float.IsNaN(Position.Y) || float.IsNaN(Position.Z))
                Position = new Vector3(0, 5, -2);
        }

        public void Draw(float dt, RenderOptions? options = null) { }
    }
}
