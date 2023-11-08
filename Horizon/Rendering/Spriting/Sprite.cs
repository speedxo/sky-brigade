using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering.Spriting.Data;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering.Spriting;

public abstract class Sprite : Entity
{
    private static int _idCounter = 0;
    private bool _hasBeenSetup = false;

    public SpriteSheet Spritesheet { get; private set; }
    public SpriteSheetAnimationManager AnimationManager { get; private set; }

    public bool ShouldDraw { get; set; } = true;
    public bool Flipped
    {
        set
        {
            if (Transform.Scale.X < 0 && value)
                return;
            if (Transform.Scale.X > 0 && !value)
                return;

            Transform.Scale = new Vector2(-Transform.Scale.X, Transform.Scale.Y);
        }
        get => Transform.Scale.X < 0;
    }
    internal bool ShouldUpdateVbo { get; private set; }

    public Vector2 Size { get; set; } = Vector2.One;
    public bool IsAnimated { get; set; }
    public string FrameName { get; private set; }

    public TransformComponent2D Transform { get; init; }

    public Sprite()
    {
        this.Transform = AddComponent<TransformComponent2D>();
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
        this.AnimationManager = AddComponent(new SpriteSheetAnimationManager(Spritesheet));

        this.FrameName = name;
        this.ID = _idCounter++;

        this.IsAnimated = AnimationManager.Animations.Any();
        _hasBeenSetup = true;
    }

    public void SetAnimation(string name)
    {
        this.FrameName = name;
    }

    public Vertex2D[] GetVertices()
    {
        if (!_hasBeenSetup)
            Engine.Logger.Log(Logging.LogLevel.Error, "[Sprite] Setup() has not been called!");

        Vector2[] uv = IsAnimated
            ? GetAnimatedTextureCoordinates(FrameName)
            : Spritesheet.GetTextureCoordinates(FrameName);

        return new Vertex2D[]
        {
            new Vertex2D(-Size.X / 2.0f, Size.Y / 2.0f, uv[0].X, uv[0].Y),
            new Vertex2D(Size.X / 2.0f, Size.Y / 2.0f, uv[1].X, uv[1].Y),
            new Vertex2D(Size.X / 2.0f, -Size.Y / 2.0f, uv[2].X, uv[2].Y),
            new Vertex2D(-Size.X / 2.0f, -Size.Y / 2.0f, uv[3].X, uv[3].Y),
        };
    }

    public Vector2 GetFrameOffset()
    {
        if (IsAnimated)
        {
            var (definition, index) = AnimationManager[FrameName];

            return (definition.Position + new Vector2(index, 0));
        }
        return Vector2.Zero;
    }
}
