using ImGuiNET;
using Silk.NET.OpenGL;
using Horizon.Rendering;
using Vector4 = System.Numerics.Vector4;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;
using Horizon.Rendering.Shapes;

namespace Horizon.Tests.Tests
{
    public class PIDTest : IEngineTest
    {
        public bool Loaded { get; set; }
        public string Name { get; set; } = "PID Controller Test";

        private Plane axisLine, marker, autoMarker;
        private bool targetMouse = false;

        public PIDTest()
        {
            Loaded = true;

            axisLine = new Plane() {
                Size = new Vector2(10, 0.1f),
                Position = new Vector3(0, 1, 0)
            };
            marker = new Plane() {
                Size = new Vector2(0.1f, 0.2f),
                Position = new Vector3(0, 1, 0)
            };
            autoMarker = new Plane() {
                Size = new Vector2(0.1f, 0.2f),
                Position = new Vector3(0, 1.5f, 0)
            };
        }

        public void Render(float dt, RenderOptions? renderOptions = null)
        {
            RenderOptions options = (renderOptions ?? RenderOptions.Default) with
            {
                Camera = RenderOptions.Default.Camera
            };

            axisLine.Draw(dt, options with
            {
                Color = Vector4.One
            });
            autoMarker.Draw(dt, options with
            {
                Color = new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
            });
            marker.Draw(dt, options with
            {
                Color = new Vector4(0.0f, 1.0f, 0.0f, 0.0f)
            });
        }

        public void RenderGui()
        {
            ImGui.DragFloat("P: ", ref controller.kP, 0.001f);
            ImGui.DragFloat("I: ", ref controller.kI, 0.001f);
            ImGui.DragFloat("D: ", ref controller.kD, 0.0001f);
            ImGui.Checkbox("Target Mouse", ref targetMouse);

            ImGui.DragFloat("Speed", ref speed, 0.1f);

            if (ImGui.Button("Reset Position"))
            {
                controller = new PIDController(0.5f, 0.1f, 0.001f);
                marker.Position = new Vector3(0, 1, 0);
            }
        }

        private PIDController controller = new PIDController(0.5f, 0.1f, 0.001f);
        private float timer = 0.0f, speed = 2.0f;

        public void Update(float dt)
        {
            timer += dt;

            float mousePos = (GameManager.Instance.Input.Mice[0].Position.X - GameManager.Instance.Window.Position.X - GameManager.Instance.Window.Size.X / 2.0f) / (100.0f);
            float autoMarkerPos = targetMouse ? mousePos : MathF.Sin(timer * speed) * 5;

            autoMarker.Position = new Vector3(autoMarkerPos, autoMarker.Position.Y, autoMarker.Position.Z);

            setMarkerPos(controller.Update(autoMarkerPos - marker.Position.X, dt));
        }

        private void setMarkerPos(float value) => marker.Position = new Vector3(marker.Position.X + value, marker.Position.Y, marker.Position.Z);

        public void Dispose()
        {
        }
    }
}