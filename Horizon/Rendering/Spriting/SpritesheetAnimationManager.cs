using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using System.Numerics;

namespace Horizon.Rendering.Spriting;

public class SpritesheetAnimationManager : IGameComponent
{
    public string Name { get; set; } = "Spritesheet Animation Manager";
    public Entity Parent { get; set; }

    public Spritesheet Spritesheet { get; private set; }

    public Dictionary<string, SpriteAnimationDefinition> Animations { get; init; }

    public SpritesheetAnimationManager()
    {
        this.Animations = new();
    }

    public (SpriteDefinition definition, int index) this[string name]
    {
        get => GetFrame(name);
    }

    public (SpriteDefinition definition, int index) GetFrame(string name)
    {
        if (!Animations.ContainsKey(name))
        {
            GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Attempt to get animation '{name}' which doesn't exist!");
            return default;
        }

        return (Animations[name].FirstFrame, Animations[name].Index);
    }

    public void AddAnimation(string name, Vector2 position, int length, float frameTime = 0.1f, Vector2? inSize = null)
    {
        if (Animations.ContainsKey(name))
        {
            GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Attempt to add animation '{name}' which already exists!");
            return;
        }

        this.Animations.Add(name, new SpriteAnimationDefinition()
        {
            Index = 0,
            Length = length,
            FirstFrame = new SpriteDefinition
            {
                Position = position,
                Size = inSize ?? Spritesheet.SpriteSize
            },
            FrameTime = frameTime
        });
    }

    public void Draw(float dt, RenderOptions? options = null)
    {
    }

    public void Initialize()
    {
        Spritesheet = Parent as Spritesheet;
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