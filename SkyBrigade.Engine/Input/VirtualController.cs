using System.Numerics;

namespace SkyBrigade.Engine.Input;

/* This will be the main virtualised controller that will be the flagship
* output of this entire convoluded yet brilliant system.
*/
public struct VirtualController
{
    public Vector2 MovementAxis { get; set; }
    public Vector2 LookingAxis { get; set; }

    public VirtualAction Actions { get; set; }
}

