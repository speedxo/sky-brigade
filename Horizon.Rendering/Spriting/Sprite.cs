using System.Numerics;
using System.Runtime.CompilerServices;
using Horizon.Core.Components;
using Horizon.Engine;

namespace Horizon.Rendering.Spriting;

public abstract class Sprite : GameObject
{
    private static int _idCounter = 0;
    private bool _hasBeenSetup = false;

    public SpriteSheet Spritesheet { get; private set; }
    public SpriteSheetAnimationManager AnimationManager { get; private set; }
    public SpriteBatch Batch { get; internal set; }

    public bool ShouldDraw { get; set; } = true;
    public bool Flipped
    {
        set
        {
            if (Transform.Size.X < 0 && value)
                return;
            if (Transform.Size.X > 0 && !value)
                return;

            Transform.Size = new Vector2(-Transform.Size.X, Transform.Size.Y);
        }
        get => Transform.Size.X < 0;
    }
    internal bool ShouldUpdateVbo { get; private set; }

    public bool IsAnimated { get; set; }
    public string FrameName { get; private set; }

    public TransformComponent2D Transform { get; init; }

    public Sprite(in Vector2 size)
    {
        this.Transform = AddComponent<TransformComponent2D>();
        this.Transform.Size = size;
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
        uint length,
        float frameTime = 0.1f,
        Vector2? inSize = null
    )
    {
        AnimationManager ??= AddComponent(new SpriteSheetAnimationManager(inSize!.Value));
        AnimationManager.AddAnimation(name, position, length, frameTime, inSize);
    }

    /// <summary>
    /// Adds a range of animations.
    /// </summary>
    public void AddAnimationRange(
        (string name, Vector2 position, uint length, float frameTime, Vector2? inSize)[] animations
    )
    {
        foreach (var (name, position, length, frameTime, inSize) in animations)
            AddAnimation(name, position, length, frameTime, inSize);
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
            //ConcurrentLogger.Instance.Log(
            //    Logging.LogLevel.Error,
            //    $"Attempt to get sprite '{name}' which doesn't exist!"
            //); TODO: FIX
            return Array.Empty<Vector2>();
        }

        // Calculate texture coordinates for the sprite
        Vector2 topLeftTexCoord = Vector2.Zero;
        Vector2 bottomRightTexCoord = topLeftTexCoord + Spritesheet.SingleSpriteSize;

        return new Vector2[]
        {
            topLeftTexCoord,
            new Vector2(bottomRightTexCoord.X, topLeftTexCoord.Y),
            bottomRightTexCoord,
            new Vector2(topLeftTexCoord.X, bottomRightTexCoord.Y)
        };
    }

    public void ConfigureSpriteSheet(SpriteSheet spriteSheet, string name)
    {
        this.Spritesheet = (spriteSheet);
        this.AnimationManager ??= AddComponent(new SpriteSheetAnimationManager(spriteSheet));

        this.FrameName = name;

        this.IsAnimated = AnimationManager.Animations.Any();

        _hasBeenSetup = true;
    }

    public void SetAnimation(string name)
    {
        this.FrameName = name;
    }

    public Vector2 GetFrameOffset()
    {
        if (IsAnimated)
        {
            var (definition, _) = AnimationManager[FrameName];

            return (definition.Position * Spritesheet.SingleSpriteSize);
        }
        return Vector2.Zero;
    }

    public uint GetFrameIndex()
    {
        if (IsAnimated)
        {
            var (_, index) = AnimationManager[FrameName];
            return index;
        }
        return 0;
    }
}
