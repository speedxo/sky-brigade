using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.Rendering;

public class RenderTarget : GameObject
{
    public FrameBufferObject FrameBuffer { get; init; }

    public RenderTarget(in FrameBufferObjectDescription desc)
    {
        FrameBuffer = Engine.Content.FrameBuffers.Create(desc).Asset;
    }

    /// <summary>
    /// Binds textures in the order they are in the dictionary.
    /// </summary>
    public void BindTextureSamplers()
    {
        uint counter = 0;
        foreach (var (_, texture) in FrameBuffer.Attachments)
        {
            Engine.GL.BindTextureUnit(counter++, texture.Handle);
        }
    }

    public override void Render(float dt)
    {
        Engine
            .GL
            .BindFramebuffer(Silk.NET.OpenGL.FramebufferTarget.Framebuffer, FrameBuffer.Handle);

        Engine.GL.DrawBuffers((uint)FrameBuffer.DrawBuffers.Length, FrameBuffer.DrawBuffers);

        Engine.GL.Viewport(0, 0, (uint)FrameBuffer.Width, (uint)FrameBuffer.Height);
        Engine.GL.ClearColor(System.Drawing.Color.Red);
        Engine.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        base.Render(dt);

        Engine.GL.BindFramebuffer(Silk.NET.OpenGL.FramebufferTarget.Framebuffer, 0);
        Engine
            .GL
            .Viewport(0, 0, (uint)Engine.Window.ViewportSize.X, (uint)Engine.Window.ViewportSize.Y);
    }
}
