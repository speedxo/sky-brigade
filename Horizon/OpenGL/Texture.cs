using Horizon.Extentions;
using Horizon.GameEntity;
using Horizon.Logging;
using Silk.NET.OpenGL;
using System.Numerics;
using Image = SixLabors.ImageSharp.Image;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;

namespace Horizon.OpenGL;

public class Texture : Entity, IDisposable
{
    public uint Handle { get; }
    public string? Path { get; init; }

    public static int count = 0;

    public int Width { get; init; }
    public int Height { get; init; }

    public Vector2 Size
    {
        get => new Vector2(Width, Height);
    }

    public unsafe Texture(string path)
    {
        if (!File.Exists(path))
            Engine.Logger.Log(LogLevel.Fatal, $"[Texture] There is no file at location '{path}'!");

        var fileName = System.IO.Path.GetFileName(path)!;
        var lastFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path))!;

        // ! We tested to ensure a file exists.

        Path = System.IO.Path.Combine(lastFolder, fileName);

        Handle = Engine.GL.GenTexture();
        Bind();

        //Loading an image using imagesharp.
        using (var img = Image.Load<Rgba32>(path))
        {
            Width = img.Width;
            Height = img.Height;

            //Reserve enough memory from the gpu for the whole image
            Engine.GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba8,
                (uint)img.Width,
                (uint)img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );

            int y = 0;
            img.ProcessPixelRows(accessor =>
            {
                //ImageSharp 2 does not store images in contiguous memory by default, so we must send the image row by row
                for (; y < accessor.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        //Loading the actual image.
                        Engine.GL.TexSubImage2D(
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
        }

        count++;
        SetParameters();

        Engine.Logger.Log(LogLevel.Info, $"Texture[{Handle}] loaded from file '{path}'!");
    }

    public Texture(uint handle)
    {
        this.Handle = handle;
        this.Path = "";

        Bind();
        int textureWidth,
            textureHeight;
        Engine.GL.GetTexLevelParameter(
            TextureTarget.Texture2D,
            0,
            GetTextureParameter.TextureWidth,
            out textureWidth
        );
        Engine.GL.GetTexLevelParameter(
            TextureTarget.Texture2D,
            0,
            GetTextureParameter.TextureHeight,
            out textureHeight
        );
        UnBind();

        this.Width = textureWidth;
        this.Height = textureHeight;
    }

    public unsafe Texture(ReadOnlySpan<byte> data, uint width, uint height)
    {
        Width = (int)width;
        Height = (int)height;

        //Generating the opengl handle;
        Handle = Engine.GL.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            Engine.GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                d
            );
            SetParameters();
        }
    }

    public unsafe Texture(Stream stream, uint width, uint height)
    {
        Width = (int)width;
        Height = (int)height;

        //Generating the opengl handle;
        Handle = Engine.GL.GenTexture();
        Bind();

        var data = StreamExtensions.ReadAllBytes(stream);
        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            Engine.GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                d
            );
            SetParameters();
        }
        stream.Close();
    }

    public unsafe Texture(uint width, uint height)
    {
        Width = (int)width;
        Height = (int)height;

        //Generating the opengl handle;
        Handle = Engine.GL.GenTexture();
        Bind();

        SetParameters();
        Engine.GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            (int)InternalFormat.Rgba32f,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.Float,
            null
        );
    }

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)GLEnum.Repeat
        );
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)GLEnum.Repeat
        );
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)GLEnum.NearestMipmapNearest
        );
        Engine.GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)GLEnum.Nearest
        );
        //Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        //Engine.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        ////Generating mipmaps.
        Engine.GL.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        Engine.GL.ActiveTexture(textureSlot);
        Engine.GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void UnBind()
    {
        Engine.GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        Engine.GL.DeleteTexture(Handle);
        Engine.Logger.Log(LogLevel.Debug, $"Texture[{Handle}] destroyed!");
        GC.SuppressFinalize(this);
    }
}
