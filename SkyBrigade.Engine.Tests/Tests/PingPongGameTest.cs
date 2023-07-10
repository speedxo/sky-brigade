using System;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
	public class PingPongGameTest : IEngineTest
	{
        public bool Loaded { get; set; }
        public string Name { get; set; } = "Ping Pong Test";

        private RenderRectangle playerPaddle, botPaddle, ball, topBar, bottomBar;
        private Camera gameCamera;
        private Vector2 direction;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            playerPaddle = new RenderRectangle() {
                Size = new Vector2(0.1f, 1.0f),
                Position = new Vector3(-5, 0, 0)
            };

            topBar = new RenderRectangle() {
                Size = new Vector2(10, 0.1f),
                Position = new Vector3(0, -4, 0)
            };
            bottomBar = new RenderRectangle()
            {
                Size = new Vector2(10, 0.1f),
                Position = new Vector3(0, 4, 0)
            };

            botPaddle = new RenderRectangle()
            {
                Size = new Vector2(0.1f, 1.0f),
                Position = new Vector3(5, 0, 0)
            };

            ball = new RenderRectangle() {
                Size = new Vector2(0.1f)
            };
            gameCamera = new Camera() { Position = new Vector3(0, 0, 10), Locked = true };

            direction = new Vector2(-1);
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            var options = RenderOptions.Default with { Camera = gameCamera };

            playerPaddle.Draw(options);
            botPaddle.Draw(options);
            topBar.Draw(options);
            bottomBar.Draw(options);
            ball.Draw(options);
        }

        public void Update(float dt)
        {
            gameCamera.Update(dt);

            bool isKeyDown(Key key) => GameManager.Instance.Input.Keyboards[0].IsKeyPressed(key);

            var pos = playerPaddle.Position;

            if (isKeyDown(Key.W))
                playerPaddle.Position += new System.Numerics.Vector3(0, 1, 0) * dt * 10;
            if (isKeyDown(Key.S))
                playerPaddle.Position += new System.Numerics.Vector3(0, -1, 0) * dt * 10;

            if (playerPaddle.Position.Y >= 3.5f)
                playerPaddle.Position = new Vector3(playerPaddle.Position.X, 3.5f, playerPaddle.Position.Z);

            if (playerPaddle.Position.Y <= -3.5f)
                playerPaddle.Position = new Vector3(playerPaddle.Position.X, -3.5f, playerPaddle.Position.Z);

            ball.Position += new Vector3(direction * dt * 2.0f, 0);

            botPaddle.Position = new Vector3(new Vector2(botPaddle.Position.X, ball.Position.Y), 0.0f);

            if (ball.CheckIntersection(topBar))
                direction.Y = 1;
            if (ball.CheckIntersection(bottomBar))
                direction.Y = -1;

            if (ball.Position.X > 4) direction.X = -direction.X;

            if (ball.Position.X < -6) ball.Position = new Vector3(0);

            if (ball.CheckIntersection(playerPaddle))
                direction.X *= -1f;

            if (ball.CheckIntersection(botPaddle))
                direction.X = -1;
        }

        public void RenderGui()
        {
            ImGui.Text($"Ball: {MathF.Round(ball.Position.X)}, {MathF.Round(ball.Position.Y)}");
            ImGui.Text($"Player: {MathF.Round(playerPaddle.Position.X)}, {MathF.Round(playerPaddle.Position.Y)}");
            ImGui.Text($"Bounds: {MathF.Round(playerPaddle.Bounds.Top, 3)}, {MathF.Round(playerPaddle.Bounds.Bottom, 3)}");
        }

        public void Dispose()
        {

        }

    }
}

