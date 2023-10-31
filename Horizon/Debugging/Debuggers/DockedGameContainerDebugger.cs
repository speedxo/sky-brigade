using Horizon.OpenGL;
using Horizon.Rendering;
using ImGuiNET;

namespace Horizon.Debugging.Debuggers
{
    public class DockedGameContainerDebugger : DebuggerComponent
    {
        public FrameBufferObject FrameBuffer { get; set; }

        public override void Initialize()
        {
            FrameBuffer = FrameBufferManager.CreateFrameBuffer(800, 600);
            FrameBuffer.IsFixed = true;
            FrameBuffer.AddAttachment(Silk.NET.OpenGL.FramebufferAttachment.ColorAttachment0);
            FrameBuffer.ContructFrameBuffer();

            Name = "Game Container";
        }

        public override void Update(float dt) { }

        public override void Draw(float dt, ref RenderOptions options)
        {
            if (Visible && ImGui.Begin("Game Container"))
            {
                ImGui.Image(
                    (nint)
                        FrameBuffer.Attachments[
                            Silk.NET.OpenGL.FramebufferAttachment.ColorAttachment0
                        ],
                    new System.Numerics.Vector2(FrameBuffer.Width, FrameBuffer.Height),
                    new System.Numerics.Vector2(0, 1),
                    new System.Numerics.Vector2(1, 0)
                );

                ImGui.End();
            }
        }

        public override void Dispose()
        {
            
        }
    }
}
