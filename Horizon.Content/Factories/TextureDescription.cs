using Horizon.Core.Content;

namespace Horizon.Content.Factories;

/// <summary>
/// A struct representing the arguments needed to create a valid texture.
/// </summary>
public readonly struct TextureDescription : IAssetDescription
{
    public readonly string Path;
    public readonly int Width, Height;
    public readonly TextureDefinition Definition;

    public TextureDescription()
    {
        Path = string.Empty;
        Definition = TextureDefinition.RgbaFloat;
    }
}
