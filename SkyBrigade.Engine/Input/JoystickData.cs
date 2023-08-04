using System.Numerics;

namespace SkyBrigade.Engine.Input
{
    /// <summary>
    /// The JoystickData struct represents input data from a joystick/gamepad.
    /// </summary>
    public struct JoystickData
    {
        /// <summary>
        /// Gets or sets the primary axis input as a Vector2.
        /// </summary>
        public Vector2 PrimaryAxis { get; set; }

        /// <summary>
        /// Gets or sets the secondary axis input as a Vector2.
        /// </summary>
        public Vector2 SecondaryAxis { get; set; }

        /// <summary>
        /// Gets or sets the trigger input as a Vector2, where X represents the left trigger and Y represents the right trigger.
        /// </summary>
        public Vector2 Triggers { get; set; }

        /// <summary>
        /// Gets or sets the VirtualAction representing various actions triggered by the joystick/gamepad.
        /// </summary>
        public VirtualAction Actions { get; set; }

        /// <summary>
        /// Gets the default JoystickData with some pre-defined values for primary axis, secondary axis, and triggers.
        /// </summary>
        public static JoystickData Default { get; } = new JoystickData
        {
            PrimaryAxis = Vector2.One,
            SecondaryAxis = Vector2.One,
            Triggers = Vector2.One
        };
    }
}
