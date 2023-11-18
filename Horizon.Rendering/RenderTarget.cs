using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    public FrameBufferObject FrameBuffer { get; protected set; }
    public Vector2 ViewportSize { get; }

    protected virtual FrameBufferObject CreateFrameBuffer(in uint width, in uint height) => GameEngine
            .Instance
            .ContentManager
            .FrameBuffers
            .Create(
                new FrameBufferObjectDescription
                {
                    Width = width,
                    Height = height,
                    Attachments = new[]
                    {
                        FramebufferAttachment.ColorAttachment0 // Albedo
                    }
                }
            )
            .Asset;

    public RenderTarget(in uint width, in uint height)
    {
        ViewportSize = new Vector2(width, height);
    }

    public override void Initialize()
    {
        FrameBuffer = CreateFrameBuffer((uint)ViewportSize.X, (uint)ViewportSize.Y);
        base.Initialize();
    }

    public override void Render(float dt, object? obj = null)
    {
        // Bind the framebuffer and its attachments
        FrameBuffer.Bind();
        Engine.GL.Disable(EnableCap.Blend);

        // set the viewport & clear screen
        FrameBuffer.Viewport();
        Engine.GL.Clear(ClearBufferMask.ColorBufferBit);

        // draw all children
        base.Render(dt);

        Engine.GL.Enable(EnableCap.Blend);
        // set to window frame buffer
        FrameBufferObject.Unbind();

        // restore window viewport
        Engine.GL.Viewport(0, 0, (uint)GameEngine.Instance.WindowManager.WindowSize.X, (uint)GameEngine.Instance.WindowManager.WindowSize.Y);
    }
}
