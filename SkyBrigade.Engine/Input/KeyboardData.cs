using System.Numerics;

namespace SkyBrigade.Engine.Input;

/* Following OOP principals i want to abstract this as much as possible
* while retaining performance, hence the stack allocations.
*/
public struct KeyboardData
{
    public Vector2 MovementDirection { get; set; }
    public Vector2 ViewingDirection { get; set; }

    public VirtualAction Actions { get; set; }
}

