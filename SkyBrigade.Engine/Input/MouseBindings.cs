using Silk.NET.Input;

namespace SkyBrigade.Engine.Input;

/* The idea here is to allow for a serialisable and customisable way to 
	 * hotswap multiple binding profiles.
	 */
public struct MouseBindings
{
    public Dictionary<MouseButton, VirtualAction> MouseActionPairs { get; set; }

    public static MouseBindings Default { get; } = new()
    {
        MouseActionPairs = new() {
            { MouseButton.Left, VirtualAction.PrimaryAction },
                { MouseButton.Right, VirtualAction.SecondaryAction }
        }
    };
}

