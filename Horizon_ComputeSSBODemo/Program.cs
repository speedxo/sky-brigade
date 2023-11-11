using Horizon;
using Horizon.Content;
using Horizon.Extentions;
using Horizon.OpenGL;
using Horizon.Rendering;
using Horizon.Rendering.Particles;
using Horizon.Rendering.Particles.Materials;
using Horizon.Rendering.Spriting;
using ImGuiNET;
using Microsoft.Extensions.Options;
using Silk.NET.OpenGL;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using Shader = Horizon.Content.Shader;
using Texture = Horizon.OpenGL.Texture;

namespace Horizon_ComputeSSBODemo
{
    internal class Program : Scene
    {
        static void Main(string[] args)
        {
            using var engine = new BasicEngine(
                GameInstanceParameters.Default with
                {
                    InitialGameScreen = typeof(Program)
                }
            );
            engine.Run();
        }

        private ParticleRenderer2Ddev particles;
        private Camera camera;

        private const int length = 1024 * 1024;
        private RenderOptions rendOptions;

        public override void Initialize()
        {
            InitializeRenderingPipeline();

            camera = AddEntity<Camera>();
            particles = AddEntity(new ParticleRenderer2Ddev(length));

            Engine.GL.ClearColor(System.Drawing.Color.CornflowerBlue);
            base.Initialize();
        }

        public override void UpdateState(float dt)
        {
            camera.Position += new Vector3(
                Engine.Input.GetVirtualController().MovementAxis * dt * 5.0f,
                0.0f
            );
            particles.SpawnPosition = new Vector2(camera.Position.X, camera.Position.Y);

            base.UpdateState(dt);
        }

        public override void Render(float dt, ref RenderOptions options)
        {
            rendOptions = options with { Camera = camera };
            base.Render(dt, ref rendOptions);
        }

        public override void DrawOther(float dt, ref RenderOptions options) { }

        public override void DrawGui(float dt)
        {
            if (ImGui.Begin("Particles"))
            {
                ImGui.SliderInt("Target", ref particles.Target, 0, particles.MaximumCount - 1);
                ImGui.Text($"Maximum: {particles.Count}");
                ImGui.End();
            }
        }

        public override void Dispose() { }
    }
}
