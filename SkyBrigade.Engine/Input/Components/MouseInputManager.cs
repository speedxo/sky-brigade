using System;
using System.Numerics;
using Silk.NET.Input;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Input.Components;

public class MouseInputManager : IGameComponent
{
    public Entity Parent { get; set; }

    public static IMouse? Mouse { get => GameManager.Instance.Input.Mice.Count > 0 ? GameManager.Instance.Input.Mice[0] : null; }
    public MouseBindings Bindings { get; private set; }

    private VirtualAction actions;
    private Vector2 direction, previousPosition;

    public void Initialize()
    {
        Bindings = MouseBindings.Default;
    }

    public MouseData GetData()
    {
        return new()
        {
            Actions = actions,
            LookingAxis = direction
        };
    }

    public void Update(float dt)
    {
        if (Mouse == null) return;

        // by coincidence our key is a key
        foreach ((MouseButton btn, VirtualAction action) in Bindings.MouseActionPairs)
        {
            if (Mouse.IsButtonPressed(btn))
            {
                actions |= action;
            }
            else
            {
                actions ^= action;
            }
        }

        var position = Mouse.Position;

        direction = position - previousPosition;

        previousPosition = position;
    }

    public void Draw(float dt, RenderOptions? options = null)
    {

    }
}

