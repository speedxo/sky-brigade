using Horizon.Core.Primitives;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;
using Texture = Horizon.OpenGL.Assets.Texture;

namespace Horizon.OpenGL.Buffers;

public class FrameBufferObject : IGLObject
{
    public Dictionary<FramebufferAttachment, Texture> Attachments { get; init; }
    public DrawBufferMode[] DrawBuffers { get; init; }

    /// <summary>
    /// Binds a specified attachment to a texture unit.
    /// </summary>
    public void BindAttachment(in FramebufferAttachment type, in uint index) =>
        ContentManager.GL.BindTextureUnit(index, Attachments[type].Handle);

    /// <summary>
    /// Binds the current frame buffer and binds its buffers to be draw to.
    /// </summary>
    public void Bind()
    {
        ContentManager.GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        ContentManager
            .GL
            .NamedFramebufferDrawBuffers(Handle, (uint)DrawBuffers.Length, (GLEnum)DrawBuffers[0]);
    }

    /// <summary>
    /// Sets viewport size to the size of the frame buffer.
    /// </summary>
    public void Viewport() => ContentManager.GL.Viewport(0, 0, Width, Height);

    public static void Unbind() =>
        ContentManager.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public uint Handle { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
}
