using System;
using Silk.NET.Input;
using System.Numerics;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Input.Components;

public class JoystickInputManager : IGameComponent
{
    public Entity Parent { get; set; }

    // Only check the primary keyboard - We don't really care abou the rest
    public static IJoystick? Joystick { get => GameManager.Instance.Input.Joysticks.Count > 0 ? GameManager.Instance.Input.Joysticks[0] : null; }
    public JoystickBindings Bindings { get; private set; }

    public bool IsConnected { get => Joystick?.IsConnected ?? false; }

    private VirtualAction actions;
    private Vector2 primaryAxis, secondaryAxis, triggers;

    public void Initialize()
    {
        Bindings = JoystickBindings.Default;
    }

    public JoystickData GetData()
    {
        return new()
        {
            Actions = actions,
            PrimaryAxis = primaryAxis,
            SecondaryAxis = secondaryAxis,
            Triggers = triggers
        };
    }

    public void Update(float dt)
    {
        if (Joystick == null || !Joystick.IsConnected)
            return;
        

        foreach ((JoystickButton key, VirtualAction action) in Bindings.ButtonActionPairs)
        {
            if (Joystick.Buttons[(int)key].Pressed)
            {
                actions |= action;
            }
            else
            {
                actions ^= action;
            }
        }

        primaryAxis = new Vector2(Joystick.Axes[0].Position, Joystick.Axes[1].Position);
        secondaryAxis = new Vector2(Joystick.Axes[2].Position, Joystick.Axes[3].Position);
        triggers = new Vector2(Joystick.Axes[4].Position, Joystick.Axes[5].Position);
    }

    public void Draw(float dt, RenderOptions? options = null)
    {

    }
}

