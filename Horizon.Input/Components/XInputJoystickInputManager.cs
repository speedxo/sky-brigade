using System.Numerics;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Silk.NET.Input;

namespace Horizon.Input.Components
{
    /// <summary>
    /// The XInputJoystickInputManager class is responsible for handling input from a joystick/gamepad.
    /// </summary>
    public class XInputJoystickInputManager : PeripheralInputManager
    {
        /// <summary>
        /// Gets the first connected joystick/gamepad, or null if none is connected.
        /// </summary>
        public static IJoystick? Joystick =>
            Manager.NativeInputContext.Joysticks.Count > 0 ? GetController() : null;

        private static IJoystick? GetController()
        {
            // FIXME yea....
            return (
                from stick in Manager.NativeInputContext.Joysticks
                where stick.IsConnected
                select stick
            ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the JoystickBindings representing the button-to-action mappings for the joystick.
        /// </summary>
        public JoystickBindings Bindings { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a joystick/gamepad is connected.
        /// </summary>
        public bool IsConnected => Joystick?.IsConnected ?? false;

        private VirtualAction actions;

        private Vector2 primaryAxis,
            secondaryAxis,
            triggers;

        /// <summary>
        /// Initializes the JoystickInputManager by setting the default JoystickBindings.
        /// </summary>
        public XInputJoystickInputManager()
        {
            Bindings = JoystickBindings.Default;
        }

        public override void Initialize() { }

        public override void SwapBuffers() { }

        /// <summary>
        /// Retrieves the current JoystickData containing input information from the joystick/gamepad.
        /// </summary>
        /// <returns>The JoystickData containing the joystick input.</returns>
        public JoystickData GetData()
        {
            return new JoystickData
            {
                Actions = actions,
                PrimaryAxis = primaryAxis,
                SecondaryAxis = secondaryAxis,
                Triggers = triggers
            };
        }

        /// <summary>
        /// Updates the JoystickInputManager, processing input from the connected joystick/gamepad.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public override void AggregateData(float dt)
        {
            actions = VirtualAction.None;

            if (Joystick == null || !Joystick.IsConnected)
                return;

            foreach ((JoystickButton key, VirtualAction action) in Bindings.ButtonActionPairs)
            {
                if (Joystick.Buttons[(int)key].Pressed)
                {
                    actions |= action;
                }
                else
                {
                    actions ^= action;
                }
            }

            primaryAxis = new Vector2(Joystick.Axes[0].Position, Joystick.Axes[1].Position);
            secondaryAxis = new Vector2(Joystick.Axes[2].Position, Joystick.Axes[3].Position);
            triggers = new Vector2(Joystick.Axes[4].Position, Joystick.Axes[5].Position);
        }
    }
}
