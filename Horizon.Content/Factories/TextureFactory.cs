using Horizon.Content.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Texture = Horizon.Content.Assets.Texture;

namespace Horizon.Content.Factories;

/// <summary>
/// Asset factory for creating textures.
/// </summary>
public class TextureFactory : IAssetFactory<Texture, TextureDescription>
{
    public static Texture Create(in TextureDescription description)
    {
        if (description.Path.CompareTo(string.Empty) != 0)
            return CreateFromFile(in description.Path, in description.Definition);
        if (description.Width + description.Height > 2)
            return CreateFromDimensions(in description.Width, in description.Height, in description.Definition);

        return Texture.Empty;
    }

    private static unsafe Texture CreateFromDimensions(in int width, in int height, in TextureDefinition definition)
    {
        var texture = new Texture
        {
            Handle = Entity.Engine.GL.GenTexture(),
            Width = (uint)width,
            Height = (uint)height
        };

        Entity.Engine.GL.ActiveTexture(TextureUnit.Texture0);
        Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        Entity
            .Engine
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
        Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, 0);
        return texture;
    }

    private static unsafe Texture CreateFromFile(in string path, in TextureDefinition definition)
    {
        using var img = Image.Load<Rgba32>(path);

        var texture = new Texture
        {
            Handle = Entity.Engine.GL.GenTexture(),
            Width = (uint)img.Width,
            Height = (uint)img.Height
        };

        Entity.Engine.GL.ActiveTexture(TextureUnit.Texture0);
        Entity.Engine.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        //Reserve enough memory from the gpu for the whole image
        Entity
            .Engine
            .GL
            .TexImage2D(
                TextureTarget.Texture2D,
                0,
                definition.InternalFormat,
                texture.Width,
                texture.Height,
                0,
                definition.PixelFormat,
                definition.PixelType,
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
                    Entity
                        .Engine
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
        return texture;
    }

    private static void SetParameters()
    {
        // Setting some texture parameters so the texture behaves as expected.
        Entity
            .Engine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int)GLEnum.Repeat
            );
        Entity
            .Engine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int)GLEnum.Repeat
            );
        Entity
            .Engine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GLEnum.NearestMipmapNearest
            );
        Entity
            .Engine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GLEnum.Nearest
            );
        //Entity.Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        //Entity.Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        ////Generating mipmaps.
        //Entity.Engine.GL.GenerateMipmap(TextureTarget.Texture2D);
    }
}
