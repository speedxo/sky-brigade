using System;
using System.Numerics;
using Silk.NET.Input;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Input.Components;

public class KeyboardManager : IGameComponent
{
    public Entity Parent { get; set; }

    // Only check the primary keyboard - We don't really care abou the rest
    public static IKeyboard? Keyboard { get => GameManager.Instance.Input.Keyboards.Count > 0 ? GameManager.Instance.Input.Keyboards[0] : null; } 
    public KeyboardBindings Bindings { get; private set; }

    private VirtualAction actions;
    private Vector2 direction;

    public void Initialize()
    {
        Bindings = KeyboardBindings.Default;    
    }

    public KeyboardData GetData()
    {
        return new ()
        {
            Actions = actions,
            MovementDirection = direction
        };
    }

    public void Update(float dt)
    {
        if (Keyboard == null) return;

        // by coincidence our key is a key
        foreach ((Key key, VirtualAction action) in Bindings.KeyActionPairs)
        {
            if (Keyboard.IsKeyPressed(key))
            {
                actions |= action;
            }
            else
            {
                actions ^= action;
            }
        }

        direction = new Vector2(
            Keyboard.IsKeyPressed(Key.D) ? 1 : Keyboard.IsKeyPressed(Key.A) ? -1 : 0,
            Keyboard.IsKeyPressed(Key.S) ? -1 : Keyboard.IsKeyPressed(Key.W) ? 1 : 0
        );
    }

    public void Draw(float dt, RenderOptions? options = null)
    {

    }
}

