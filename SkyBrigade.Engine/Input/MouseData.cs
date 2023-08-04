using System.Numerics;

namespace SkyBrigade.Engine.Input;

/* Following OOP principals i want to abstract this as much as possible
* while retaining performance, hence the stack allocations.
*/
public struct MouseData
{
    public Vector2 LookingAxis { get; set; }

    public VirtualAction Actions { get; set; }
}

