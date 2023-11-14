using Silk.NET.OpenGL;

namespace Horizon.Content.Factories;

/// <summary>
/// Low level texture definition.
/// </summary>
public readonly struct TextureDefinition
{
    public readonly InternalFormat InternalFormat { get; init; }
    public readonly PixelFormat PixelFormat { get; init; }
    public readonly PixelType PixelType { get; init; }

    public static TextureDefinition RgbaUnsignedByte { get; } = new TextureDefinition
    {
        InternalFormat = InternalFormat.Rgba,
        PixelFormat = PixelFormat.Rgba,
        PixelType = PixelType.UnsignedByte
    };
    public static TextureDefinition RgbaFloat { get; } = new TextureDefinition
    {
        InternalFormat = InternalFormat.Rgba,
        PixelFormat = PixelFormat.Rgba,
        PixelType = PixelType.Float
    };
}
