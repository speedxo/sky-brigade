using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Horizon.Core.Primitives;
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
        FrameBuffer = GameEngine.Instance.ContentManager.FrameBuffers.Create(desc).Asset;
    }

    /// <summary>
    /// Binds textures in the order they are in the dictionary.
    /// </summary>
    public void BindTextureSamplers()
    {
        uint counter = 0;
        foreach (var (_, texture) in FrameBuffer.Attachments)
        {
            GameEngine.Instance.GL.BindTextureUnit(counter++, texture.Handle);
        }
    }

    public override void Render(float dt, object? obj = null)
    {
        GameEngine.Instance
            .GL
            .BindFramebuffer(Silk.NET.OpenGL.FramebufferTarget.Framebuffer, FrameBuffer.Handle);

        GameEngine.Instance.GL.DrawBuffers((uint)FrameBuffer.DrawBuffers.Length, FrameBuffer.DrawBuffers);

        GameEngine.Instance.GL.Viewport(0, 0, (uint)FrameBuffer.Width, (uint)FrameBuffer.Height);
        GameEngine.Instance.GL.ClearColor(System.Drawing.Color.Red);
        GameEngine.Instance.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        base.Render(dt);

        GameEngine.Instance.GL.BindFramebuffer(Silk.NET.OpenGL.FramebufferTarget.Framebuffer, 0);
        GameEngine.Instance
            .GL
            .Viewport(
                0,
                0,
                (uint)GameEngine.Instance.WindowManager.ViewportSize.X,
                (uint)GameEngine.Instance.WindowManager.ViewportSize.Y
            );
    }
}
