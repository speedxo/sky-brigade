using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering;
using SkyBrigade.Engine.Input.Components;
using Silk.NET.SDL;
using System.Numerics;

namespace SkyBrigade.Engine.Input
{
    /// <summary>
    /// The InputManager class is responsible for managing input from various sources (keyboard, mouse, joystick)
    /// and providing a unified VirtualController that aggregates input data from these sources. (well said jesus)
    /// </summary>
    public class InputManager : Entity
    {
        /// <summary>
        /// Gets or sets a value indicating whether input is captured by the InputManager.
        /// </summary>
        public bool CaptureInput { get; set; }

        /// <summary>
        /// Gets the KeyboardManager responsible for handling keyboard input.
        /// </summary>
        public KeyboardManager KeyboardManager { get; init; }

        /// <summary>
        /// Gets the MouseInputManager responsible for handling mouse input.
        /// </summary>
        public MouseInputManager MouseManager { get; init; }

        /// <summary>
        /// Gets the JoystickInputManager responsible for handling joystick input.
        /// </summary>
        public JoystickInputManager JoystickManager { get; init; }

        private VirtualController VirtualController = default;

        /// <summary>
        /// Initializes a new instance of the InputManager class with default values.
        /// </summary>
        public InputManager()
        {
            KeyboardManager = AddComponent<KeyboardManager>();
            MouseManager = AddComponent<MouseInputManager>();
            JoystickManager = AddComponent<JoystickInputManager>();

            CaptureInput = true;
        }

        /// <summary>
        /// Updates the InputManager, aggregating input data from various sources into the VirtualController.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public override void Update(float dt)
        {
            base.Update(dt);

            var keyboardData = KeyboardManager.GetData();
            var mouseData = MouseManager.GetData();
            var joystickData = JoystickManager.IsConnected ? JoystickManager.GetData() : JoystickData.Default;

            VirtualController.Actions = keyboardData.Actions | mouseData.Actions | joystickData.Actions;
            VirtualController.MovementAxis = keyboardData.MovementDirection * (JoystickManager.IsConnected ? joystickData.PrimaryAxis : Vector2.One);
            VirtualController.LookingAxis = mouseData.LookingAxis * (JoystickManager.IsConnected ? joystickData.SecondaryAxis : Vector2.One);
        }

        /// <summary>
        /// Gets the VirtualController providing unified input data from various sources.
        /// </summary>
        /// <returns>The VirtualController instance.</returns>
        public VirtualController GetVirtualController() => CaptureInput ? VirtualController : default;

        /// <summary>
        /// Disposes of any resources used by the InputManager.
        /// </summary>
        public void Dispose()
        {
            // Add any necessary cleanup logic here, if required.
        }
    }
}
