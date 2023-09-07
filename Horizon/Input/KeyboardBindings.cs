using Silk.NET.Input;

namespace Horizon.Input
{
    /// <summary>
    /// The KeyboardBindings struct represents a serializable and customizable way to hotswap multiple binding profiles for keyboard input.
    /// </summary>
    public struct KeyboardBindings
    {
        /// <summary>
        /// Gets or sets the dictionary of Key and VirtualAction pairs representing key-to-action mappings.
        /// </summary>
        public Dictionary<Key, VirtualAction> KeyActionPairs { get; set; }

        /// <summary>
        /// Gets the default KeyboardBindings with some pre-defined key-to-action mappings.
        /// </summary>
        public static KeyboardBindings Default { get; } =
            new KeyboardBindings
            {
                KeyActionPairs = new Dictionary<Key, VirtualAction>
                {
                    { Key.E, VirtualAction.Interact },
                    { Key.Escape, VirtualAction.Pause }
                }
            };
    }
}
