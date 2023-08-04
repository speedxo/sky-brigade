using System;
using System.Net.Sockets;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
	public class CameraComponent : Camera, IGameComponent
    {
        public Entity Parent { get; set; }

        public Vector3 Front { get; set; }

        public TransformComponent Transform { get; set; }

        private float CameraYaw = -90f;
        private float CameraPitch = 0f;

#pragma warning disable CS8601 // ooo woo im Roslyn sheesh fancy c# lexer
        public void Initialize()
        {
            Locked = false;

            // again we can do this (see Entity.cs lines 17 & 18)
            Transform = Parent.GetComponent<TransformComponent>();
        }
#pragma warning restore CS8601 // SID, SHUT THE FUCK UP

        float lookSensitivity = 0.1f;
        private void UpdateMouse()
        {
            var controller = GameManager.Instance.InputManager.GetVirtualController();

            var xOffset = (controller.LookingAxis.X) * lookSensitivity;
            var yOffset = (controller.LookingAxis.Y) * lookSensitivity;


            CameraYaw += xOffset;
            CameraPitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

            Transform.Rotation = new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch)),
                MathF.Sin(MathHelper.DegreesToRadians(CameraPitch)),
                MathF.Sin(MathHelper.DegreesToRadians(CameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(CameraPitch))
                );

            Front = Vector3.Normalize(Transform.Rotation);
        }

        public void Draw(float dt, RenderOptions? options = null)
        {

        }

        public override void Update(float dt)
        {
            if (!GameManager.Instance.IsInputCaptured) return;

            UpdateMouse();
            View = Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Transform.Front, CameraUp);
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)GameManager.Instance.Window.FramebufferSize.X / (float)GameManager.Instance.Window.FramebufferSize.Y, 0.1f, 100.0f);
        }
    }
}

