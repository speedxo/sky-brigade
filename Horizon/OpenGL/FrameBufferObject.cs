using Horizon.GameEntity;
using Silk.NET.OpenGL;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Horizon.OpenGL;

public class FrameBufferObject : Entity, IDisposable
{
    public Dictionary<FramebufferAttachment, uint> Attachments { get; init; }

    public uint Handle { get; protected set; }

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    private bool _requiresResize = false;
    private Vector2 _newSize;

    [Pure]
    protected static (
        InternalFormat internalFormat,
        PixelFormat pixelFormat
    ) GetCorespondingAttachmentFormats(FramebufferAttachment attachment)
    {
        return attachment switch
        {
            FramebufferAttachment.DepthStencilAttachment
                => (InternalFormat.DepthStencil, PixelFormat.DepthStencil),
            FramebufferAttachment.DepthAttachment
                => (InternalFormat.DepthComponent, PixelFormat.DepthComponent),
            _ => (InternalFormat.Rgba, PixelFormat.Rgba)
        };
    }

    public void AddAttachment(FramebufferAttachment attachment)
    {
        if (Attachments.ContainsKey(attachment))
            return;

        var (internalFormat, pixelFormat) = GetCorespondingAttachmentFormats(attachment);

        Attachments.Add(
            attachment,
            GenerateTexture(Width, Height, internalFormat, pixelFormat: pixelFormat)
        );
    }

    public void Resize(int newWidth, int newHeight)
    {
        _requiresResize = true;
        _newSize = new Vector2(newWidth, newHeight);
    }

    public unsafe FrameBufferObject(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        // Create a framebuffer object
        Handle = Engine.GL.GenFramebuffer();

        Attachments = new Dictionary<FramebufferAttachment, uint>();
    }

    public bool ContructFrameBuffer()
    {
        Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

        //Debug.Assert(Engine.GL.GetError() == GLEnum.None);

        foreach (var (attachment, texture) in Attachments)
        {
            Engine.GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                attachment,
                TextureTarget.Texture2D,
                texture,
                0
            );
            Engine.GL.DrawBuffer((DrawBufferMode)attachment);
        }
        // Check if the framebuffer is complete
        if (
            Engine.GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
            != GLEnum.FramebufferComplete
        )
        {
            Engine.Logger.Log(
                Logging.LogLevel.Error,
                "Framebuffer is incomplete." + Engine.GL.GetError().ToString()
            );
            return false;
        }

        // Unbind the framebuffer
        Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return true;
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
    private unsafe uint GenerateTexture(
        int w,
        int h,
        InternalFormat internalFormat = InternalFormat.Rgba,
        PixelType pixelType = PixelType.Float,
        PixelFormat pixelFormat = PixelFormat.Rgba
    )
    {
        uint texture = Engine.GL.GenTexture();

        Engine.GL.BindTexture(TextureTarget.Texture2D, texture);

        Engine.GL.TexImage2D(
            GLEnum.Texture2D,
            0,
            (int)internalFormat,
            (uint)w,
            (uint)h,
            0,
            pixelFormat,
            pixelType,
            null
        );
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear
        );
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear
        );

        Engine.GL.BindTexture(TextureTarget.Texture2D, 0);

        Engine.Content.AddUnmanagedTexture(texture);

        return texture;
    }

    public void Bind()
    {
        if (_requiresResize)
        {
            Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            List<FramebufferAttachment> attachmentTypes = new();
            foreach (var (type, texture) in Attachments)
            {
                Engine.GL.DeleteTexture(texture);
                attachmentTypes.Add(type);
            }
            Engine.GL.DeleteFramebuffer(Handle);

            this.Width = (int)_newSize.X;
            this.Height = (int)_newSize.Y;

            Attachments.Clear();
            // Create a framebuffer object
            Handle = Engine.GL.GenFramebuffer();

            foreach (var attachment in attachmentTypes)
            {
                AddAttachment(attachment);
            }

            attachmentTypes.Clear();

            ContructFrameBuffer();
        }

        Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        foreach (var (attachment, _) in Attachments)
        {
            Engine.GL.DrawBuffer((DrawBufferMode)attachment);
        }
    }

    public void Unbind()
    {
        Engine.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var (_, texture) in Attachments)
        {
            Engine.GL.DeleteTexture(texture);
        }
    }
}
