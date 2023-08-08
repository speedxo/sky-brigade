using Silk.NET.OpenGL;
using SkyBrigade.Engine.Logging;
using Image = SixLabors.ImageSharp.Image;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;

namespace SkyBrigade.Engine.OpenGL;

public class Texture : IDisposable
{
    public uint Handle { get; }
    private GL _gl;
    public string? Path { get; init; }

    public static int count = 0;

    public int Width { get; init; }
    public int Height { get; init; }

    public unsafe Texture(GL gl, string path)
    {
        var fileName = System.IO.Path.GetFileName(path);
        var lastFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));

        Path = System.IO.Path.Combine(lastFolder, fileName);

        _gl = gl;

        Handle = _gl.GenTexture();
        Bind();

        //Loading an image using imagesharp.
        using (var img = Image.Load<Rgba32>(path))
        {
            Width = img.Width;
            Height = img.Height;

            //Reserve enough memory from the gpu for the whole image
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

            int y = 0;
            img.ProcessPixelRows(accessor =>
            {
                //ImageSharp 2 does not store images in contiguous memory by default, so we must send the image row by row
                for (; y < accessor.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        //Loading the actual image.
                        gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                }
            });
        }

        count++;
        SetParameters();

        GameManager.Instance.Logger.Log(LogLevel.Info, $"Texture[{Handle}] loaded from file '{path}'!");
    }
    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        //Saving the gl instance.
        _gl = gl;

        Width = (int)width;
        Height = (int)height;

        //Generating the opengl handle;
        Handle = _gl.GenTexture();
        Bind();

        //We want the ability to create a texture using data generated from code aswell.
        fixed (void* d = &data[0])
        {
            //Setting the data of a texture.
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        //Setting some texture perameters so the texture behaves as expected.
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.NearestMipmapNearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        //Generating mipmaps.
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        //When we bind a texture we can choose which textureslot we can bind it to.
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose()
    {
        //In order to dispose we need to delete the opengl handle for the texure.
        _gl.DeleteTexture(Handle);
        GameManager.Instance.Logger.Log(LogLevel.Debug, $"Texture[{Handle}] destroyed!");
        GC.SuppressFinalize(this);
    }
}