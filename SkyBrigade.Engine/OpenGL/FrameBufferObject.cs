using Silk.NET.OpenGL;
using System.Diagnostics;

namespace SkyBrigade.Engine.OpenGL;

public class FrameBufferObject
{
    /// <summary>
    /// The framebuffer object.
    /// </summary>
    private uint fbo;

    /// <summary>
    /// The texture attachment.
    /// </summary>

    /// <summary>
    /// The frag attachment.
    /// </summary>
    public uint TextureAttachment { get; }

    /// <summary>
    /// The surface normal attachment.
    /// </summary>
    public uint FragAttachment { get; }

    /// <summary>
    /// The texture normal attachment.
    /// </summary>
    public uint SurfaceNormalAttachment { get; }

    /// <summary>
    /// The depth attachment.
    /// </summary>
    public uint TextureNormalAttachment { get; }

    public uint DepthAttachment { get; }
    /// <summary>
    /// The gl.
    /// </summary>

    private GL gl;
    /// <summary>
    /// Gets the width.
    /// </summary>
    /// <value>The width.</value>

    /// <summary>
    /// Gets the height.
    /// </summary>
    /// <value>The height.</value>
    public int Width { get; private set; }

    public int Height { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:glTest.OpenGL.FrameBufferObject"/> class.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>

    public unsafe FrameBufferObject(GL gl, int width, int height)
    {
        this.gl = gl;
        this.Width = width;
        this.Height = height;

        // Create a framebuffer object
        fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        Debug.Assert(gl.GetError() == GLEnum.None);

        TextureAttachment = GenerateTexture(width, height, InternalFormat.Rgba, PixelType.UnsignedByte);
        AttachTexture(FramebufferAttachment.ColorAttachment0, TextureAttachment);
        Debug.Assert(gl.GetError() == GLEnum.None);

        FragAttachment = GenerateTexture(width, height);
        AttachTexture(FramebufferAttachment.ColorAttachment1, FragAttachment);
        Debug.Assert(gl.GetError() == GLEnum.None);

        SurfaceNormalAttachment = GenerateTexture(width, height);
        AttachTexture(FramebufferAttachment.ColorAttachment2, SurfaceNormalAttachment);
        Debug.Assert(gl.GetError() == GLEnum.None);

        TextureNormalAttachment = GenerateTexture(width, height);
        AttachTexture(FramebufferAttachment.ColorAttachment3, TextureNormalAttachment);
        Debug.Assert(gl.GetError() == GLEnum.None);

        DepthAttachment = GenerateTexture(width, height,
            internalFormat: InternalFormat.DepthComponent,
            pixelFormat: PixelFormat.DepthComponent);
        AttachTexture(FramebufferAttachment.DepthAttachment, DepthAttachment);
        Debug.Assert(gl.GetError() == GLEnum.None);

        gl.DrawBuffers(new DrawBufferMode[] { DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1, DrawBufferMode.ColorAttachment2, DrawBufferMode.ColorAttachment3 });

        // Check if the framebuffer is complete
        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is incomplete." + gl.GetError().ToString());
        }

        // TODO: Use a custom exception.
        // Unbind the framebuffer
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>
    /// Generates the texture.
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="w">The width.</param>
    /// <param name="h">The height.</param>
    /// <param name="internalFormat">Internal format.</param>
    /// <param name="pixelType">Pixel type.</param>
    /// <param name="pixelFormat">Pixel format.</param>
    private unsafe uint GenerateTexture(int w, int h, InternalFormat internalFormat = InternalFormat.Rgba, PixelType pixelType = PixelType.Float, PixelFormat pixelFormat = PixelFormat.Rgba)
    {
        uint texture = gl.GenTexture();

        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexImage2D(GLEnum.Texture2D, 0, (int)internalFormat, (uint)w, (uint)h, 0, pixelFormat, pixelType, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return texture;
    }

    /// <summary>
    /// Attaches the texture.
    /// </summary>
    /// <param name="atatchment">Atatchment.</param>
    /// <param name="texture">Texture.</param>
    private void AttachTexture(FramebufferAttachment atatchment, uint texture)
    {
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, atatchment, TextureTarget.Texture2D, texture, 0);
    }

    public void Bind()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
    }

    public void Unbind()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Dispose()
    {
        gl.DeleteTexture(TextureAttachment);
        gl.DeleteTexture(FragAttachment);
        gl.DeleteTexture(SurfaceNormalAttachment);
        gl.DeleteTexture(TextureNormalAttachment);
        gl.DeleteTexture(DepthAttachment);
        gl.DeleteFramebuffer(fbo);
    }
}