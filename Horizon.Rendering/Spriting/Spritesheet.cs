using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// A specialized <see cref="Texture"/> with specific additions such as sprite definitions, animation management and soon to be refactored backend rendering code.
/// </summary>
/// <seealso cref="Horizon.OpenGL.Texture" />
public class SpriteSheet : Texture
{
    public int ID { get; private set; }

    public Dictionary<string, SpriteDefinition> Sprites { get; init; }
    public Vector2 SpriteSize { get; init; }
    public Vector2 SingleSpriteSize { get; init; }

    public SpriteSheet()
    {
        this.Sprites = new();
    }

    public static SpriteSheet FromTexture(in Texture texture, in Vector2 spriteSize)
    {
        return new SpriteSheet()
        {
            Handle = texture.Handle,
            Width = texture.Width,
            Height = texture.Height,
            SpriteSize = spriteSize,
            SingleSpriteSize = spriteSize / new Vector2(texture.Width, texture.Height)
        };
    }

    /// <summary>
    /// Defines a sprite by name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="pos">The position.</param>
    /// <param name="size">The size.</param>
    public void AddSprite(string name, Vector2 pos, Vector2? size = null)
    {
        if (Sprites.ContainsKey(name))
        {
            //Entity.Engine.Logger.Log(
            //    Logging.LogLevel.Error,
            //    $"Attempt to add sprite '{name}' which already exists!"
            //); TODO: FIX
            return;
        }

        this.Sprites.Add(name, new SpriteDefinition { Position = pos, Size = size ?? SpriteSize });
    }

    /// <summary>
    /// Gets the static texture coordinates.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Vector2[] GetTextureCoordinates(string name)
    {
        if (!Sprites.TryGetValue(name, out var sprite))
        {
            //Entity.Engine.Logger.Log(
            //    Logging.LogLevel.Error,
            //    $"Attempt to get sprite '{name}' which doesn't exist!"
            //); TODO: FIX
            return Array.Empty<Vector2>();
        }

        // Calculate texture coordinates for the sprite
        Vector2 topLeftTexCoord = sprite.Position / new Vector2(Width, Height); // todo
        Vector2 bottomRightTexCoord = (sprite.Position + sprite.Size) / new Vector2(Width, Height);

        return new Vector2[]
        {
            topLeftTexCoord,
            new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
            bottomRightTexCoord,
            new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
        };
    }
}
