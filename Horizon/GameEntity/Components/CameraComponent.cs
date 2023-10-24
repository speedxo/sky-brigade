using Horizon.Rendering;
using System.Numerics;

namespace Horizon.GameEntity.Components
{
    /// <summary>
    /// CameraComponent class represents a camera in the game.
    /// </summary>
    [RequiresComponent(typeof(TransformComponent))]
    public class CameraComponent : Camera, IGameComponent
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent entity of the camera.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Gets or sets the camera's front direction vector.
        /// </summary>
        public Vector3 Front { get; private set; }

        /// <summary>
        /// Gets or sets the TransformComponent attached to the camera.
        /// </summary>
        public TransformComponent Transform { get; private set; }

        private float CameraYaw = -90f;
        private float CameraPitch = 0f;

#pragma warning disable CS8601 // ooo woo im Roslyn sheesh fancy c# lexer

        /// <summary>
        /// Initializes the CameraComponent.
        /// </summary>
        public void Initialize()
        {
            Locked = false;
            // again we can do this (see Entity.cs lines 17 & 18)
            Transform = Parent.GetComponent<TransformComponent>();
        }

#pragma warning restore CS8601 // SID, SHUT THE FUCK UP

        private float lookSensitivity = 0.1f;

        private void UpdateMouse()
        {
            var controller = Entity.Engine.Input.GetVirtualController();

            var xOffset = (controller.LookingAxis.X) * lookSensitivity;
            var yOffset = (controller.LookingAxis.Y) * lookSensitivity;

            CameraYaw += xOffset;
            CameraPitch -= yOffset;

            // We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

            Transform.Rotation = new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(CameraYaw))
                    * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch)),
                MathF.Sin(MathHelper.DegreesToRadians(CameraPitch)),
                MathF.Sin(MathHelper.DegreesToRadians(CameraYaw))
                    * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch))
            );

            Front = Vector3.Normalize(Transform.Rotation);
        }

        public void Draw(float dt, ref RenderOptions options)
        {
            // Not used.
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public override void Update(float dt)
        {
            // FIXME somehow figure out a better way to pass around the Engine.

            if (!Entity.Engine.Input.CaptureInput)
                return;

            UpdateMouse();
            View = Matrix4x4.CreateLookAt(
                Transform.Position,
                Transform.Position + Transform.Front,
                CameraUp
            );

            Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(CameraZoom),
                (float)Entity.Engine.Window.ViewportSize.X
                    / (float)Entity.Engine.Window.ViewportSize.Y,
                0.1f,
                100.0f
            );
        }
    }
}
