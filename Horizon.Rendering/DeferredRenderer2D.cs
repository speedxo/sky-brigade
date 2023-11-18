using Horizon.Engine;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;

using Silk.NET.OpenGL;

namespace Horizon.Rendering;

public class DeferredRenderer2D : Renderer2D
{
    protected override FrameBufferObject CreateFrameBuffer(in uint width, in uint height) => GameEngine
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
                        FramebufferAttachment.ColorAttachment0, // Albedo
                        FramebufferAttachment.ColorAttachment1, // Normal
                        FramebufferAttachment.ColorAttachment2 // Fragment position
                    }
                }
            )
            .Asset;

    protected override Renderer2DTechnique CreateTechnique() => new DeferredRenderer2DTechnique(FrameBuffer);

    public DeferredRenderer2D(in uint width, in uint height)
        : base(width, height) { }
}
