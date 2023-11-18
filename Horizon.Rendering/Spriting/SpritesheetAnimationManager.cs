using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using Horizon.Core;
using Horizon.Core.Components;

namespace Horizon.Rendering.Spriting;

/// <summary>
/// An internal component used to keep track of animated regions of a spritesheet.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Components.IGameComponent" />
public class SpriteSheetAnimationManager : IGameComponent
{
    public string Name { get; set; } = "SpriteSheet Animation Manager";
    public Entity Parent { get; set; }
    public bool Enabled { get; set; }

    public ConcurrentDictionary<string, SpriteAnimationDefinition> Animations { get; init; }
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
            //Entity.ConcurrentLogger.Instance.Log(
            //    Logging.LogLevel.Error,
            //    $"Attempt to get animation '{name}' which doesn't exist!"
            //); TODO: FIX
            return default;
        }

        return (value.FirstFrame, value.Index);
    }

    public void UpdatePhysics(float dt) { }

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
            //Entity.ConcurrentLogger.Instance.Log(
            //    Logging.LogLevel.Error,
            //    $"Attempt to add animation '{name}' which already exists!"
            //); TODO: FIX
            return;
        }

        this.Animations.TryAdd(
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
                Fuzz = Random.Shared.NextSingle() * 0.2f + 0.8f
            }
        );
    }

    public void Render(float dt, object? obj = null) { }

    public void Initialize() { }

    public void UpdateState(float dt)
    {
        foreach (var name in Animations.Keys)
        {
            var frame = Animations[name];

            if (frame.Length < 1)
            {
                frame.Index = 0;
                continue;
            }

            frame.Timer += dt * frame.Fuzz;

            if (frame.Timer >= frame.FrameTime)
            {
                frame.Timer = 0.0f;
                frame.Index = (frame.Index + 1) % frame.Length;
            }

            Animations[name] = frame;
        }
    }
}
