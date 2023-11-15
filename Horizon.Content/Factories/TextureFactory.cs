using Horizon.Core.Assets;
using Horizon.Core.Content;
using Horizon.Core.Primitives;
using Horizon.OpenGL.Buffers;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Texture = Horizon.Core.Assets.Texture;

namespace Horizon.Content.Factories;

public readonly struct BufferDescription : IAssetDescription
{
    public readonly BufferTargetARB Type { get; init; }
    public readonly bool IsStorageBuffer { get; init; }
    public readonly uint Size { get; init; }
    public readonly BufferStorageMask StorageMasks { get; init; }

    public static BufferDescription ArrayBuffer { get; } = new BufferDescription { 
        Type = BufferTargetARB.ArrayBuffer,
        IsStorageBuffer = false,
        Size = 0
    };
}

public class BufferFactory : IAssetFactory<BufferObject, BufferDescription>
{
    public static unsafe BufferObject Create(in BufferDescription description)
    {
        var buffer = new BufferObject { Handle = BaseGameEngine.GL.CreateBuffer(), Type = description.Type };

        if (description.IsStorageBuffer)
            BaseGameEngine.GL.NamedBufferStorage(buffer.Handle, description.Size, null, description.StorageMasks);

        return buffer;
    }
}

/// <summary>
/// Asset factory for creating instances of <see cref="Texture"/>.
/// </summary>
public class TextureFactory : IAssetFactory<Texture, TextureDescription>
{
    public static Texture Create(in TextureDescription description)
    {
        if (description.Path.CompareTo(string.Empty) != 0)
            return CreateFromFile(in description.Path, in description.Definition);
        if (description.Width + description.Height > 2)
            return CreateFromDimensions(in description.Width, in description.Height, in description.Definition);

        return Texture.Invalid;
    }

    private static unsafe Texture CreateFromDimensions(in int width, in int height, in TextureDefinition definition)
    {
        var texture = new Texture
        {
            Handle = BaseGameEngine.GL.GenTexture(),
            Width = (uint)width,
            Height = (uint)height
        };

        BaseGameEngine.GL.ActiveTexture(TextureUnit.Texture0);
        BaseGameEngine.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        BaseGameEngine
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
        BaseGameEngine.GL.BindTexture(TextureTarget.Texture2D, 0);
        return texture;
    }

    private static unsafe Texture CreateFromFile(in string path, in TextureDefinition definition)
    {
        using var img = Image.Load<Rgba32>(path);

        var texture = new Texture
        {
            Handle = BaseGameEngine.GL.GenTexture(),
            Width = (uint)img.Width,
            Height = (uint)img.Height
        };

        BaseGameEngine.GL.ActiveTexture(TextureUnit.Texture0);
        BaseGameEngine.GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        //Reserve enough memory from the gpu for the whole image
        BaseGameEngine
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
                    BaseGameEngine
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
        BaseGameEngine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int)GLEnum.Repeat
            );
        BaseGameEngine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int)GLEnum.Repeat
            );
        BaseGameEngine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GLEnum.NearestMipmapNearest
            );
        BaseGameEngine
            .GL
            .TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GLEnum.Nearest
            );
        //BaseGameEngine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        //BaseGameEngine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        ////Generating mipmaps.
        //BaseGameEngine.GL.GenerateMipmap(TextureTarget.Texture2D);
    }
}
