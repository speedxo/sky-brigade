using Silk.NET.Input;
using SkyBrigade.Engine;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.Prefabs.Character;

public partial class CharacterController
{
    [RequiresComponent(typeof(TransformComponent))]
    [RequiresComponent(typeof(CameraComponent))]
    public class CharacterMovementController : IGameComponent
    {
        public string Name { get; set; }

        public Entity Parent { get; set; }
        public CharacterController Controller { get; private set; }
        public TransformComponent Transform { get; private set; }
        public CameraComponent Camera { get; private set; }
        public CharacterMovementControllerConfig Config { get; private set; }

        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
        public Vector3 Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }
        public Vector3 Front { get => Transform.Front; }

#pragma warning disable CS8601, CS8602 // shut UP.
        public void Initialize()
        {
            Config = CharacterMovementControllerConfig.Default;

            Controller = Parent as CharacterController;

            Transform = Parent.GetComponent<TransformComponent>();
            Transform.Position = new Vector3(0, 5, -2);
        }
#pragma warning restore CS8601, CS8602 // SHUT UP

        public void Update(float dt)
        {
            DoLocomotion(dt);
        }

        private void DoLocomotion(float dt)
        {
            var moveSpeed = Config.BaseMovementSpeed * dt;

            var virtualController = GameManager.Instance.InputManager.GetVirtualController();


            Position += Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY)) * moveSpeed * virtualController.MovementAxis.X;
            Position += moveSpeed * Front * virtualController.MovementAxis.Y;

            // there apears to be some funky wunky happenings with the underlying
            // glfw window wrapper silk.net is using sooooooooooooo
            if (float.IsNaN(Position.X) || float.IsNaN(Position.Y) || float.IsNaN(Position.Z))
                Position = new Vector3(0, 5, -2);
        }

        public void Draw(float dt, RenderOptions? options = null)
        {

        }
    }
}