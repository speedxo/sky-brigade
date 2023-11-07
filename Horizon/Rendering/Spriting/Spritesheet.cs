using Horizon.OpenGL;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// A specialized <see cref="Texture"/> with specific additions such as sprite definitions, animation management and soon to be refactored backend rendering code.
/// </summary>
/// <seealso cref="Horizon.OpenGL.Texture" />
public class SpriteSheet : Texture
{
    // This block gotta go
    private static int _idCounter = 0;
    private int SpriteCounter = 0;

    internal int GetNewSpriteId()
    {
        return SpriteCounter++;
    }

    public Dictionary<string, SpriteDefinition> Sprites { get; init; }
    public Vector2 SpriteSize { get; init; }
    public Vector2 SingleSpriteSize { get; init; }
    public SpriteSheetAnimationManager AnimationManager { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteSheet"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="spriteSize">Size of the sprite.</param>
    public SpriteSheet(string path, Vector2 spriteSize)
        : base(path)
    {
        this.ID = _idCounter++;

        this.SpriteSize = spriteSize;
        this.Sprites = new();

        SingleSpriteSize = SpriteSize / Size;

        AnimationManager = this.AddComponent<SpriteSheetAnimationManager>();
    }

    /// <summary>
    /// Adds the animation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="position">The position in normalized coordinates.</param>
    /// <param name="length">The animation length in frames.</param>
    /// <param name="frameTime">The frame time.</param>
    /// <param name="inSize">Custom frame size.</param>
    public void AddAnimation(
        string name,
        Vector2 position,
        int length,
        float frameTime = 0.1f,
        Vector2? inSize = null
    ) => AnimationManager.AddAnimation(name, position, length, frameTime, inSize);

    /// <summary>
    /// Adds a range of animations.
    /// </summary>
    public void AddAnimationRange(
        (string name, Vector2 position, int length, float frameTime, Vector2? inSize)[] animations
    )
    {
        foreach (var (name, position, length, frameTime, inSize) in animations)
            AddAnimation(name, position, length, frameTime, inSize);
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
            Engine.Logger.Log(
                Logging.LogLevel.Error,
                $"Attempt to add sprite '{name}' which already exists!"
            );
            return;
        }

        this.Sprites.Add(name, new SpriteDefinition { Position = pos, Size = size ?? SpriteSize });
    }

    /// <summary>
    /// Gets the texture coordinates with respect to the configured sprite sheet.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Vector2[] GetAnimatedTextureCoordinates(string name)
    {
        if (!AnimationManager.Animations.TryGetValue(name, out var sprite))
        {
            Engine.Logger.Log(
                Logging.LogLevel.Error,
                $"Attempt to get sprite '{name}' which doesn't exist!"
            );
            return Array.Empty<Vector2>();
        }

        // Calculate texture coordinates for the sprite
        Vector2 topLeftTexCoord = Vector2.Zero;
        Vector2 bottomRightTexCoord = topLeftTexCoord + SingleSpriteSize;

        return new Vector2[]
        {
            topLeftTexCoord,
            new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
            bottomRightTexCoord,
            new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
        };
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
            Engine.Logger.Log(
                Logging.LogLevel.Error,
                $"Attempt to get sprite '{name}' which doesn't exist!"
            );
            return Array.Empty<Vector2>();
        }

        // Calculate texture coordinates for the sprite
        Vector2 topLeftTexCoord = sprite.Position / Size;
        Vector2 bottomRightTexCoord = (sprite.Position + sprite.Size) / Size;

        return new Vector2[]
        {
            topLeftTexCoord,
            new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
            bottomRightTexCoord,
            new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
        };
    }

    internal void ResetSpriteCounter()
    {
        SpriteCounter = 0;
    }
}
