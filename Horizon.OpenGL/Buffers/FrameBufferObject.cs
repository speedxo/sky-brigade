using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using Texture = Horizon.OpenGL.Assets.Texture;

namespace Horizon.OpenGL.Buffers;

public class FrameBufferObject : IGLObject
{
    public Dictionary<FramebufferAttachment, Texture> Attachments { get; init; }
    public DrawBufferMode[] DrawBuffers { get; init; }

    public uint Handle { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
}
