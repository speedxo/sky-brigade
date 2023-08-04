using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Input.Components;
using Silk.NET.SDL;
using System.Numerics;

namespace SkyBrigade.Engine.Input;

public class InputManager : Entity
{
    public bool CaptureInput { get; set; }

    public KeyboardManager KeyboardManager { get; private set; }
    public MouseInputManager MouseManager { get; private set; }
    public JoystickInputManager JoystickManager { get; private set; }

    private VirtualController VirtualController = default;

    public InputManager()
    {
        KeyboardManager = AddComponent<KeyboardManager>();
        MouseManager = AddComponent<MouseInputManager>();
        JoystickManager = AddComponent<JoystickInputManager>();

        CaptureInput = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        var keyboardData = KeyboardManager.GetData();
        var mouseData = MouseManager.GetData();
        var joystickData = JoystickManager.IsConnected ? JoystickManager.GetData() : JoystickData.Default;

        VirtualController.Actions = keyboardData.Actions | mouseData.Actions | joystickData.Actions;
        VirtualController.MovementAxis = keyboardData.MovementDirection * (JoystickManager.IsConnected ? joystickData.PrimaryAxis : Vector2.One);
        VirtualController.LookingAxis = mouseData.LookingAxis * (JoystickManager.IsConnected ? joystickData.SecondaryAxis : Vector2.One);
    }

    public VirtualController GetVirtualController() => CaptureInput ? VirtualController : default;

    public void Dispose()
    {

    }
}

