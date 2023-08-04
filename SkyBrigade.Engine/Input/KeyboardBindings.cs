using Silk.NET.Input;

namespace SkyBrigade.Engine.Input;

/* The idea here is to allow for a serialisable and customisable way to 
* hotswap multiple binding profiles.
*/
public struct KeyboardBindings
{
    public Dictionary<Key, VirtualAction> KeyActionPairs { get; set; }

    public static KeyboardBindings Default { get; } = new()
    {
        KeyActionPairs = new() {
            { Key.E, VirtualAction.Interact },
            { Key.Escape, VirtualAction.Pause }
            }
    };
}

