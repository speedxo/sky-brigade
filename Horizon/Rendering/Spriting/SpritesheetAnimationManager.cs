﻿using Horizon.GameEntity;
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

    public SpriteSheet Spritesheet { get; private set; }

    public Dictionary<string, SpriteAnimationDefinition> Animations { get; init; }

    public SpriteSheetAnimationManager()
    {
        this.Animations = new();
    }

    public (SpriteDefinition definition, int index) this[string name]
    {
        get => GetFrame(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (SpriteDefinition definition, int index) GetFrame(string name)
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
        int length,
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
                    Size = inSize ?? Spritesheet.SpriteSize
                },
                FrameTime = frameTime,
            }
        );
    }

    public void Draw(float dt, ref RenderOptions options) { }

    public void Initialize()
    {
        Spritesheet = (SpriteSheet)Parent;
    }

    public void Update(float dt)
    {
        foreach (var name in Animations.Keys)
        {
            var frame = Animations[name];

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
