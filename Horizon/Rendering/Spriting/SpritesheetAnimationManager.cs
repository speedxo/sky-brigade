using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// An internal component used to keep track of animated regions of a spritesheet.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Components.IGameComponent" />
public class SpriteSheetAnimationManager : IGameComponent
{
    public string Name { get; set; } = "SpriteSheet Animation Manager";
    public Entity Parent { get; set; }

    public Dictionary<string, SpriteAnimationDefinition> Animations { get; init; }
    public Vector2 SpriteSize { get; init; }

    public SpriteSheetAnimationManager(in Vector2 spriteSize)
    {
        this.SpriteSize = spriteSize;
        this.Animations = new();
    }

    public SpriteSheetAnimationManager(in SpriteSheet sheet)
        : this(sheet.SpriteSize) { }

    public (SpriteDefinition definition, uint index) this[string name]
    {
        get => GetFrame(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (SpriteDefinition definition, uint index) GetFrame(string name)
    {
        if (!Animations.TryGetValue(name, out SpriteAnimationDefinition value))
        {
            Entity.Engine.Logger.Log(
                Logging.LogLevel.Error,
                $"Attempt to get animation '{name}' which doesn't exist!"
            );
            return default;
        }

        return (value.FirstFrame, value.Index);
    }

    public void AddAnimation(
        string name,
        Vector2 position,
        uint length,
        float frameTime = 0.1f,
        Vector2? inSize = null
    )
    {
        if (Animations.ContainsKey(name))
        {
            Entity.Engine.Logger.Log(
                Logging.LogLevel.Error,
                $"Attempt to add animation '{name}' which already exists!"
            );
            return;
        }

        this.Animations.Add(
            name,
            new SpriteAnimationDefinition()
            {
                Index = 0,
                Length = length,
                FirstFrame = new SpriteDefinition
                {
                    Position = position,
                    Size = inSize ?? SpriteSize
                },
                FrameTime = frameTime,
            }
        );
    }

    public void Draw(float dt, ref RenderOptions options) { }

    public void Initialize() { }

    public void Update(float dt)
    {
        foreach (var name in Animations.Keys)
        {
            var frame = Animations[name];

            if (frame.Length < 1)
            {
                frame.Index = 0;
                continue;
            }

            frame.Timer += dt;

            if (frame.Timer >= frame.FrameTime)
            {
                frame.Timer = 0.0f;
                frame.Index = (frame.Index + 1) % frame.Length;
            }

            Animations[name] = frame;
        }
    }
}
