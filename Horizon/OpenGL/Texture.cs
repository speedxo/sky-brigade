using Horizon.Extentions;
using Horizon.Logging;
using Silk.NET.OpenGL;
using System.Numerics;
using Image = SixLabors.ImageSharp.Image;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;

namespace Horizon.OpenGL;

public class Texture : IDisposable
{
    public uint Handle { get; }
    public string? Path { get; init; }
    public string? Name { get; internal set; }

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
            GameManager.Instance.Logger.Log(
                LogLevel.Fatal,
                $"[Texture] There is no file at location '{path}'!"
            );

        var fileName = System.IO.Path.GetFileName(path)!;
        var lastFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path))!;

        // ! We tested to ensure a file exists.

        Path = System.IO.Path.Combine(lastFolder, fileName);

        Handle = GameManager.Instance.Gl.GenTexture();
        Bind();

        //Loading an image using imagesharp.
        using (var img = Image.Load<Rgba32>(path))
        {
            Width = img.Width;
            Height = img.Height;

            //Reserve enough memory from the gpu for the whole image
            GameManager.Instance.Gl.TexImage2D(
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
                        GameManager.Instance.Gl.TexSubImage2D(
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

        GameManager.Instance.Logger.Log(
            LogLevel.Info,
            $"Texture[{Handle}] loaded from file '{path}'!"
        );
    }

    public Texture(uint handle)
    {
        this.Handle = handle;
        this.Path = "";

        Bind();
        int textureWidth,
            textureHeight;
        GameManager.Instance.Gl.GetTexLevelParameter(
            TextureTarget.Texture2D,
            0,
            GetTextureParameter.TextureWidth,
            out textureWidth
        );
        GameManager.Instance.Gl.GetTexLevelParameter(
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
        Handle = GameManager.Instance.Gl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            GameManager.Instance.Gl.TexImage2D(
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
        Handle = GameManager.Instance.Gl.GenTexture();
        Bind();

        var data = StreamExtensions.ReadAllBytes(stream);
        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            GameManager.Instance.Gl.TexImage2D(
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

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)GLEnum.Repeat
        );
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)GLEnum.Repeat
        );
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)GLEnum.NearestMipmapNearest
        );
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)GLEnum.Nearest
        );
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureBaseLevel,
            0
        );
        GameManager.Instance.Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMaxLevel,
            8
        );
        //Generating mipmaps.
        GameManager.Instance.Gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        GameManager.Instance.Gl.ActiveTexture(textureSlot);
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void UnBind()
    {
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        GameManager.Instance.Gl.DeleteTexture(Handle);
        GameManager.Instance.Logger.Log(LogLevel.Debug, $"Texture[{Handle}] destroyed!");
        GC.SuppressFinalize(this);
    }
}
