using System.Numerics;

using Horizon.Engine;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;

using Silk.NET.OpenGL;

namespace Horizon.Rendering;

/// <summary>
/// Class providing a rendering and post processing pipeline for 2D sprite oriented rendering, specializing in extra functionality for pixel art.
/// "Please be advised that due to poor design you can only instantiate this object with an active GL instance, ie. inside an IInitialize.Initialize() function." - here for historic reasons, no longer the case dw i guess.
/// </summary>
public class Renderer2D : GameObject
{
    public FrameBufferObject FrameBuffer { get => frameBuffer; private set => frameBuffer = value; }
    public RenderRectangle RenderRectangle { get => renderRectangle; private set => renderRectangle = value; }
    public Vector2 ViewportSize { get; init; }

    protected virtual Renderer2DTechnique CreateTechnique() => new(FrameBuffer);
    protected virtual FrameBufferObject CreateFrameBuffer(in uint width, in uint height) => GameEngine
            .Instance
            .ObjectManager
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

    private FrameBufferObject frameBuffer;
    private RenderRectangle renderRectangle;

    public Renderer2D(in uint width, in uint height)
    {
        ViewportSize = new Vector2(width, height);
    }

    public override void Initialize()
    {
        base.Initialize();

        // i am aware we just went from uint -> float!! -> uint but fuck it we ball.
        FrameBuffer = CreateFrameBuffer((uint)ViewportSize.X, (uint)ViewportSize.Y);

        RenderRectangle = new(CreateTechnique());
        PushToInitializationQueue(renderRectangle);
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

        // draw framebuffer to window
        RenderRectangle.Render(dt);
    }
}
