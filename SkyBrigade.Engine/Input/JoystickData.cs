using System.Numerics;

namespace SkyBrigade.Engine.Input;

/* Following OOP principals i want to abstract this as much as possible
* while retaining performance, hence the stack allocations.
*/
public struct JoystickData
{
    public Vector2 PrimaryAxis { get; set; }
    public Vector2 SecondaryAxis { get; set; }
    public Vector2 Triggers { get; set; }

    public VirtualAction Actions { get; set; }

    public static JoystickData Default { get; } = new()
    {
        PrimaryAxis = Vector2.One,
        SecondaryAxis = Vector2.One,
        Triggers = Vector2.One
    };
}

