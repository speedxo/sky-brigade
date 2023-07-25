using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.Tests.Tests
{
    public class PingPongGameTest : IEngineTest
    {
        public bool Loaded { get; set; }
        public string Name { get; set; } = "Ping Pong Test";

        private Rendering.Plane playerPaddle, botPaddle, ball, topBar, bottomBar;
        private Camera gameCamera;
        private Vector2 direction;
        private float speed = 5.0f;
        private bool isAutomatic = true;

        public void LoadContent(GL gl)
        {
            if (Loaded) return;

            playerPaddle = new Rendering.Plane() {
                Size = new Vector2(0.1f, 1.0f),
                Position = new Vector3(-5, 0, 0)
            };

            topBar = new Rendering.Plane()
            {
                Size = new Vector2(10, 0.1f),
                Position = new Vector3(0, -4, 0)
            };
            bottomBar = new Rendering.Plane()
            {
                Size = new Vector2(10, 0.1f),
                Position = new Vector3(0, 4, 0)
            };

            botPaddle = new Rendering.Plane()
            {
                Size = new Vector2(0.1f, 0.5f),
                Position = new Vector3(5, 0, 0)
            };

            ball = new Rendering.Plane()
            {
                Size = new Vector2(0.1f)
            };
            gameCamera = new Camera() { Position = new Vector3(0, 0, 10), Locked = true };

            direction = new Vector2(-1);
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            var options = RenderOptions.Default with { Camera = gameCamera, Color = Vector4.One };

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

            if (!isAutomatic)
            {
                if (isKeyDown(Key.W))
                    playerPaddle.Position += new System.Numerics.Vector3(0, 1, 0) * dt * 10;
                if (isKeyDown(Key.S))
                    playerPaddle.Position += new System.Numerics.Vector3(0, -1, 0) * dt * 10;

                if (playerPaddle.Position.Y >= 3.5f)
                    playerPaddle.Position = new Vector3(playerPaddle.Position.X, 3.5f, playerPaddle.Position.Z);

                if (playerPaddle.Position.Y <= -3.5f)
                    playerPaddle.Position = new Vector3(playerPaddle.Position.X, -3.5f, playerPaddle.Position.Z);
            }

            ball.Position += new Vector3(direction * dt * speed, 0);

            updateBot(dt);

            if (ball.CheckIntersection(topBar))
                direction.Y = 1;
            if (ball.CheckIntersection(bottomBar))
                direction.Y = -1;

            if (MathF.Abs(ball.Position.X) > 6) ball.Position = new Vector3(0);

            if (ball.CheckIntersection(playerPaddle))
                direction.X = 1f;

            if (ball.CheckIntersection(botPaddle))
                direction.X = -1;
        }
        PIDController controller = new PIDController(0.8f, 0.1f, 0.001f);
        private void updateBot(float dt)
        {
            var error = ball.Position.Y - botPaddle.Position.Y;
            var output = controller.Update(error, dt);
            botPaddle.Position = new Vector3(botPaddle.Position.X, botPaddle.Position.Y + output, botPaddle.Position.Z);
            if (isAutomatic) playerPaddle.Position = new Vector3(playerPaddle.Position.X, botPaddle.Position.Y, playerPaddle.Position.Z);
        }

        public void RenderGui()
        {
            ImGui.DragFloat("P", ref controller.kP, 0.001f);
            ImGui.DragFloat("I", ref controller.kI, 0.001f);
            ImGui.DragFloat("D", ref controller.kD, 0.0001f);
            ImGui.DragFloat("Speed", ref speed, 0.01f);
            ImGui.Checkbox("Auto", ref isAutomatic);

            ImGui.Text($"Delta Error: {ball.Position.Y - botPaddle.Position.Y}");
        }

        public void Dispose()
        {
        }
    }
}