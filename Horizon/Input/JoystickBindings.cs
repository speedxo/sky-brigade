namespace Horizon.Input
{
    /// <summary>
    /// The JoystickBindings struct represents a serializable and customizable way to hotswap multiple binding profiles for joysticks.
    /// </summary>
    public struct JoystickBindings
    {
        /// <summary>
        /// Gets or sets the dictionary of JoystickButton and VirtualAction pairs representing button-to-action mappings.
        /// </summary>
        public Dictionary<JoystickButton, VirtualAction> ButtonActionPairs { get; set; }

        /// <summary>
        /// Gets the default JoystickBindings with some pre-defined button-to-action mappings.
        /// </summary>
        public static JoystickBindings Default { get; } = new JoystickBindings
        {
            ButtonActionPairs = new Dictionary<JoystickButton, VirtualAction>
            {
                { JoystickButton.Y, VirtualAction.Back },
                { JoystickButton.X, VirtualAction.Interact },
                { JoystickButton.Start, VirtualAction.Pause }
            }
        };
    }
}