using System;
using ImGuiNET;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Debugging.Debuggers
{
    public class DockedGameContainerDebugger : IGameComponent
	{
        public string Name { get; set; } = "Game Container";
        public Entity Parent { get; set; }
        public bool Visible = true;

        public FrameBufferObject FrameBuffer { get; set; }

        public void Initialize()
        {
            FrameBuffer = new FrameBufferObject(800, 600);
            FrameBuffer.AddAttachment(Silk.NET.OpenGL.FramebufferAttachment.ColorAttachment0);
            FrameBuffer.ContructFrameBuffer();
        }

        public void Update(float dt)
        {

        }

        public void Draw(float dt, RenderOptions? options = null)
        {
            if (Visible && ImGui.Begin("Game Container"))
            {
                ImGui.Image((nint)FrameBuffer.Attachments[Silk.NET.OpenGL.FramebufferAttachment.ColorAttachment0], new System.Numerics.Vector2(FrameBuffer.Width, FrameBuffer.Height), new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));

                ImGui.End();
            }
        }
    }
}

