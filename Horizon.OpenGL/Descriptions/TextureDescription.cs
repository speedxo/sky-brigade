using Horizon.Content.Descriptions;
using Silk.NET.OpenGL;

namespace Horizon.OpenGL.Descriptions;

/// <summary>
/// A struct representing the arguments needed to create a valid texture.
/// </summary>
public readonly struct TextureDescription : IAssetDescription
{
    public readonly string Path { get; init; }
    public readonly uint Width { get; init; }
    public readonly uint Height { get; init; }
    public readonly TextureDefinition Definition { get; init; }

    public TextureDescription()
    {
        Path = string.Empty;
        Definition = TextureDefinition.RgbaFloat;
    }
}
