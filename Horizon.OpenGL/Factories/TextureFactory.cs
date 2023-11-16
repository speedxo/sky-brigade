using Horizon.Content;
using Horizon.Content.Descriptions;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.OpenGL.Managers;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Texture = Horizon.OpenGL.Assets.Texture;

namespace Horizon.OpenGL.Factories;

/// <summary>
/// Asset factory for creating instances of <see cref="Texture"/>.
/// </summary>
public class TextureFactory : IAssetFactory<Texture, TextureDescription>
{
    public static AssetCreationResult<Texture> Create(in TextureDescription description)
    {
        if (description.Path.CompareTo(string.Empty) != 0)
            return CreateFromFile(description.Path, description.Definition);
        if (description.Width + description.Height > 2)
            return CreateFromDimensions(
                description.Width,
                description.Height,
                description.Definition
            );


        return new AssetCreationResult<Texture>()
        {
            Asset = Texture.Invalid,
            Status = AssetCreationStatus.Failed,
        };
    }

    private static unsafe AssetCreationResult<Texture> CreateFromDimensions(
        in uint width,
        in uint height,
        in TextureDefinition definition
    )
    {
        var texture = new Texture
        {
            Handle = ContentManager.GL.GenTexture(),
            Width = (uint)width,
            Height = (uint)height
        };

        ContentManager.GL.ActiveTexture(TextureUnit.Texture0);
        ContentManager.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        ContentManager
            .GL
            .TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)definition.InternalFormat,
                texture.Width,
                texture.Height,
                0,
                definition.PixelFormat,
                definition.PixelType,
                null
            );
        SetParameters();
        ContentManager.GL.BindTexture(TextureTarget.Texture2D, 0);

        return new() { Asset = texture, Status = AssetCreationStatus.Success };
    }

    private static unsafe AssetCreationResult<Texture> CreateFromFile(
        in string path,
        in TextureDefinition definition
    )
    {
        using var img = Image.Load<Rgba32>(path);

        var texture = new Texture
        {
            Handle = ContentManager.GL.CreateTexture(TextureTarget.Texture2D),
            Width = (uint)img.Width,
            Height = (uint)img.Height
        };

        ContentManager.GL.ActiveTexture(TextureUnit.Texture0);
        ContentManager.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        //Reserve enough memory from the gpu for the whole image
        ContentManager
            .GL
            .TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                texture.Width,
                texture.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

        int y = 0;
        img.ProcessPixelRows(accessor =>
        {
            //ImageSharp 2 does not store images in contiguous memory by default, so we must send the image row by row :cry:
            for (; y < accessor.Height; y++)
            {
                fixed (void* data = accessor.GetRowSpan(y))
                {
                    //Loading the actual image.
                    ContentManager
                        .GL
                        .TexSubImage2D(
                            TextureTarget.Texture2D,
                            0,
                            0,
                            y,
                            (uint)accessor.Width,
                            1,
                            PixelFormat.Rgba,
                            PixelType.UnsignedByte,
                            data
                        );
                }
            }
        });
        SetParameters();
        ContentManager.GL.BindTexture(TextureTarget.Texture2D, 0);

        return new() { Asset = texture, Status = AssetCreationStatus.Success };
    }

    private static void SetParameters()
    {
        // Setting some texture parameters so the texture behaves as expected.
        ContentManager
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int)GLEnum.ClampToEdge
            );
        ContentManager
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int)GLEnum.ClampToEdge
            );
        ContentManager
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GLEnum.Nearest
            );
        ContentManager
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GLEnum.Nearest
            );
        //ContentManager.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        //ContentManager.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        ////Generating mipmaps.
        //ContentManager.GL.GenerateMipmap(TextureTarget.Texture2D);
    }
}
