using Horizon.Content.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Descriptions;

public readonly struct FrameBufferObjectDescription : IAssetDescription
{
    public readonly uint Width { get; init; }
    public readonly uint Height { get; init; }
    public readonly FramebufferAttachment[] Attachments { get; init; }

    /// <summary>
    /// Returns a frame buffer description with a single color buffer.
    /// </summary>
    public static FrameBufferObjectDescription FromSize(uint width, uint height) =>
        new()
        {
            Width = width,
            Height = height,
            Attachments = new FramebufferAttachment[] { FramebufferAttachment.ColorAttachment0 }
        };
}
