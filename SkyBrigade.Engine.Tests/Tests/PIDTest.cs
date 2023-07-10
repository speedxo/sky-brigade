using System;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json.Linq;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Tests.Tests
{
	public class PIDTest : IEngineTest
	{
        public bool Loaded { get; set; }
        public string Name { get; set; } = "PID Controller Test";

        RenderRectangle axisLine, marker, autoMarker;

        public void LoadContent(GL gl)
        {
            Loaded = true;

            axisLine = new RenderRectangle(inPos: new Vector3(0, 1, 0), inSize:new Vector2(10, 0.1f));
            marker = new RenderRectangle(inPos: new Vector3(0, 1, 0), inSize: new Vector2(0.1f, 0.2f));
            autoMarker = new RenderRectangle(inPos: new Vector3(0, 1.5f, 0), inSize: new Vector2(0.1f, 0.2f));
        }

        public void Render(float dt, GL gl, RenderOptions? renderOptions = null)
        {
            axisLine.Draw();
            autoMarker.Draw(RenderOptions.Default with
            {
                Color = new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
            });
            marker.Draw(RenderOptions.Default with {
                 Color = new Vector4(0.0f, 1.0f, 0.0f, 0.0f)
            });
        }

        public void RenderGui()
        {
            ImGui.DragFloat("P: ", ref controller.kP, 0.001f);
            ImGui.DragFloat("I: ", ref controller.kI, 0.001f);
            ImGui.DragFloat("D: ", ref controller.kD, 0.0001f);

            ImGui.DragFloat("Speed: ", ref speed, 0.1f);

            if (ImGui.Button("Reset Position"))
            {
                controller = new PIDController(0.5f, 0.1f, 0.001f);
                marker.Position = new Vector3(0, 1, 0);
            }
        }
        PIDController controller = new PIDController(0.5f, 0.1f, 0.001f);
        float timer = 0.0f, speed = 2.0f;
        public void Update(float dt)
        {
            timer += dt;

            float mousePos = (GameManager.Instance.Input.Mice[0].Position.X - GameManager.Instance.Window.Position.X - GameManager.Instance.Window.Size.X / 2.0f) / (100.0f);

            float autoMarkerPos = MathF.Sin(timer * speed) * 5.0f;
            autoMarker.Position = new Vector3(autoMarkerPos, autoMarker.Position.Y, autoMarker.Position.Z);

            setMarkerPos(controller.Update(autoMarkerPos - marker.Position.X, dt));
        }

        void setMarkerPos(float value) => marker.Position = new Vector3(marker.Position.X + value, marker.Position.Y, marker.Position.Z);

        public void Dispose()
        {

        }

    }
}

