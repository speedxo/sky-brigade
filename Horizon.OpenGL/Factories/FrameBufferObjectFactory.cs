using System.Diagnostics.Contracts;
using Horizon.Content;
using Horizon.Content.Descriptions;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Factories;

public class FrameBufferObjectFactory
    : IAssetFactory<FrameBufferObject, FrameBufferObjectDescription>
{
    public static unsafe AssetCreationResult<FrameBufferObject> Create(
        in FrameBufferObjectDescription description
    )
    {
        // delegates textrue creation to the texture manager.
        var attachments = CreateFrameBufferAttachments(
            description.Width,
            description.Height,
            description.Attachments
        );

        var drawBuffers = new DrawBufferMode[attachments.Count];
        for (int i = 0; i < drawBuffers.Length; i++)
            drawBuffers[i] = (DrawBufferMode)description.Attachments[i];

        var buffer = new FrameBufferObject
        {
            Handle = ContentManager.GL.CreateFramebuffer(),
            Width = description.Width,
            Height = description.Height,
            Attachments = attachments,
            DrawBuffers = drawBuffers
        };

        ContentManager.GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Handle);

        foreach (var (attachment, texture) in attachments)
            ContentManager.GL.NamedFramebufferTexture(buffer.Handle, attachment, texture.Handle, 0);

        // Check if the framebuffer is complete
        if (
            ContentManager.GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)
            != GLEnum.FramebufferComplete
        )
        {
            // Unbind the framebuffer
            ContentManager.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // cleanup
            ContentManager.GL.DeleteFramebuffer(buffer.Handle);
            foreach (var (_, texture) in attachments)
                ContentManager.Instance.Textures.Remove(texture);

            return new AssetCreationResult<FrameBufferObject>
            {
                Asset = buffer,
                Message = "Framebuffer is incomplete.",
                Status = AssetCreationStatus.Failed
            };
        }

        // Unbind the framebuffer
        ContentManager.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return new() { Asset = buffer, Status = AssetCreationStatus.Success };
    }

    private static Dictionary<FramebufferAttachment, Assets.Texture> CreateFrameBufferAttachments(
        uint width,
        uint height,
        FramebufferAttachment[] attachmentTypes
    )
    {
        var attachments = new Dictionary<FramebufferAttachment, Assets.Texture>();

        foreach (var attachmentType in attachmentTypes)
        {
            var (internalFormat, pixelFormat) = GetCorrespondingAttachmentFormats(attachmentType);
            attachments.Add(
                attachmentType,
                ContentManager
                    .Instance
                    .Textures
                    .Create(
                        new TextureDescription
                        {
                            Width = width,
                            Height = height,
                            Definition = new TextureDefinition
                            {
                                InternalFormat = internalFormat,
                                PixelFormat = pixelFormat,
                                PixelType = PixelType.Float
                            }
                        }
                    )
                    .Asset // TODO: error checking skipped.
            );
        }

        return attachments;
    }

    [Pure]
    protected static (
        InternalFormat internalFormat,
        PixelFormat pixelFormat
    ) GetCorrespondingAttachmentFormats(FramebufferAttachment attachment)
    {
        return attachment switch
        {
            FramebufferAttachment.DepthStencilAttachment
                => (InternalFormat.DepthStencil, PixelFormat.DepthStencil),
            FramebufferAttachment.DepthAttachment
                => (InternalFormat.DepthComponent, PixelFormat.DepthComponent),
            _ => (InternalFormat.Rgba32f, PixelFormat.Rgba) // TODO:  somehow customize this
        };
    }
}
