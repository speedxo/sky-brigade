﻿using Silk.NET.Input;

namespace Horizon.Input
{
    /// <summary>
    /// The MouseBindings struct represents a serializable and customizable way to hotswap multiple binding profiles for mouse input.
    /// </summary>
    public struct MouseBindings
    {
        /// <summary>
        /// Gets or sets the dictionary of MouseButton and VirtualAction pairs representing mouse-button-to-action mappings.
        /// </summary>
        public Dictionary<MouseButton, VirtualAction> MouseActionPairs { get; set; }

        /// <summary>
        /// Gets the default MouseBindings with some pre-defined mouse-button-to-action mappings.
        /// </summary>
        public static MouseBindings Default { get; } =
            new MouseBindings
            {
                MouseActionPairs = new Dictionary<MouseButton, VirtualAction>
                {
                    { MouseButton.Left, VirtualAction.PrimaryAction },
                    { MouseButton.Right, VirtualAction.SecondaryAction }
                }
            };
    }
}
