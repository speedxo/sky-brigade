using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using Silk.NET.Input;
using System.Numerics;

namespace Horizon.Input.Components
{
    /// <summary>
    /// The XInputJoystickInputManager class is responsible for handling input from a joystick/gamepad.
    /// </summary>
    public class XInputJoystickInputManager : IGameComponent
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent Entity that owns this XInputJoystickInputManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Gets the first connected joystick/gamepad, or null if none is connected.
        /// </summary>
        public static IJoystick? Joystick =>
            Entity.Engine.Input.NativeInputContext.Joysticks.Count > 0 ? GetController() : null;

        private static IJoystick? GetController()
        {
            // FIXME yea....
            return (
                from stick in Entity.Engine.Input.NativeInputContext.Joysticks
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
        public void Initialize()
        {
            Bindings = JoystickBindings.Default;
        }

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
        public void UpdateState(float dt)
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

        public void UpdatePhysics(float dt) { }

        /// <summary>
        /// Draws the JoystickInputManager, not used for joystick input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Render(float dt, ref RenderOptions options)
        {
            // Not used for joystick input.
        }
    }
}
