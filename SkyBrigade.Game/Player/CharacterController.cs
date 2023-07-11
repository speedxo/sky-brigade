using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Game.Player
{
    public class CharacterController : Entity
    {
        public TransformComponent Transform { get; private set; }

        // how do position and rotation have the same number of letters????
        public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }

        public Vector3 Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }

        public Camera Camera { get; protected set; }

        public CharacterController(Camera cam)
        {
            // we do this to share the same instance of a camera with the gamescreen.
            this.Camera = cam;

            Transform = AddComponent<TransformComponent>();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}