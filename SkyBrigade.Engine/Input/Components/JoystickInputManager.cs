using System;
using Silk.NET.Input;
using System.Numerics;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Input.Components
{
    /// <summary>
    /// The JoystickInputManager class is responsible for handling input from a joystick/gamepad.
    /// </summary>
    public class JoystickInputManager : IGameComponent
    {
        /// <summary>
        /// Gets or sets the parent Entity that owns this JoystickInputManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Gets the first connected joystick/gamepad, or null if none is connected.
        /// </summary>
        public static IJoystick? Joystick => GameManager.Instance.Input.Joysticks.Count > 0 ? GameManager.Instance.Input.Joysticks[0] : null;

        /// <summary>
        /// Gets the JoystickBindings representing the button-to-action mappings for the joystick.
        /// </summary>
        public JoystickBindings Bindings { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a joystick/gamepad is connected.
        /// </summary>
        public bool IsConnected => Joystick?.IsConnected ?? false;

        private VirtualAction actions;
        private Vector2 primaryAxis, secondaryAxis, triggers;

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
        public void Update(float dt)
        {
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

        /// <summary>
        /// Draws the JoystickInputManager, not used for joystick input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Draw(float dt, RenderOptions? options = null)
        {
            // Not used for joystick input.
        }
    }
}
