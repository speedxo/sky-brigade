using Horizon.GameEntity;
using Horizon.Input.Components;
using Silk.NET.Input;
using System.Numerics;

namespace Horizon.Input
{
    /// <summary>
    /// The InputManager class is responsible for managing input from various sources (keyboard, mouse, joystick)
    /// and providing a unified VirtualController that aggregates input data from these sources. (well said jesus)
    /// </summary>
    public class InputManager : Entity, IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether input is captured by the InputManager.
        /// </summary>
        public bool CaptureInput { get; set; }

        /// <summary>
        /// The window's native input context.
        /// </summary>
        public IInputContext NativeInputContext { get; init; }

        /// <summary>
        /// Gets the KeyboardManager responsible for handling keyboard input.
        /// </summary>
        public KeyboardManager KeyboardManager { get; init; }

        /// <summary>
        /// Gets the MouseInputManager responsible for handling mouse input.
        /// </summary>
        public MouseInputManager MouseManager { get; init; }

        /// <summary>
        /// Gets the XInputJoystickManager responsible for handling joystick input.
        /// </summary>
        public XInputJoystickInputManager XInputJoystickManager { get; init; }

        /// <summary>
        /// Gets the DualSenseInputManager responsible for handling joystick input.
        /// </summary>
        public DualSenseInputManager DualSenseInputManager { get; init; }

        private VirtualController VirtualController = default;
        private VirtualController PreviousVirtualController = default;

        /// <summary>
        /// Initializes a new instance of the InputManager class with default values.
        /// </summary>
        public InputManager(in EngineWindowManager window)
        {
            NativeInputContext = window.GetInput();

            KeyboardManager = AddComponent<KeyboardManager>();
            MouseManager = AddComponent<MouseInputManager>();
            XInputJoystickManager = AddComponent<XInputJoystickInputManager>();
            DualSenseInputManager = AddComponent<DualSenseInputManager>();

            CaptureInput = false;
        }

        /// <summary>
        /// Updates the InputManager, aggregating input data from various sources into the VirtualController.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public override void Update(float dt)
        {
            base.Update(dt);

            var keyboardData = KeyboardManager.Data;

            var mouseData = MouseManager.GetData();
            var joystickData = XInputJoystickManager.IsConnected
                ? XInputJoystickManager.GetData()
                : JoystickData.Default;
            var dualsenseData = DualSenseInputManager.GetData();

            PreviousVirtualController = VirtualController;

            VirtualController.Actions =
                keyboardData.Actions
                | mouseData.Actions
                | joystickData.Actions
                | dualsenseData.Actions;

            VirtualController.MovementAxis =
                keyboardData.MovementDirection
                + (XInputJoystickManager.IsConnected ? joystickData.PrimaryAxis : Vector2.Zero)
                + (DualSenseInputManager.HasController ? dualsenseData.PrimaryAxis : Vector2.Zero);
            if (VirtualController.MovementAxis.LengthSquared() > 1.0f)
                VirtualController.MovementAxis = Vector2.Normalize(VirtualController.MovementAxis);

            VirtualController.LookingAxis =
                mouseData.LookingAxis
                + (XInputJoystickManager.IsConnected ? joystickData.SecondaryAxis : Vector2.Zero)
                + (
                    DualSenseInputManager.HasController ? dualsenseData.SecondaryAxis : Vector2.Zero
                );
        }

        /// <summary>
        /// Gets the VirtualController providing unified input data from various sources.
        /// </summary>
        /// <returns>The VirtualController instance.</returns>
        public VirtualController GetVirtualController() =>
            CaptureInput ? VirtualController : default;

        /// <summary>
        /// Gets the last frames VirtualController providing unified input data from various sources.
        /// </summary>
        /// <returns>The VirtualController instance.</returns>
        public VirtualController GetPreviousVirtualController() =>
            CaptureInput ? PreviousVirtualController : default;

        public bool IsPressed(VirtualAction action) => GetVirtualController().IsPressed(action);

        public bool WasPressed(VirtualAction action) =>
            GetVirtualController().IsPressed(action)
            && !GetPreviousVirtualController().IsPressed(action);

        /// <summary>
        /// Disposes of any resources used by the InputManager.
        /// </summary>
        public void Dispose()
        {
            DualSenseInputManager.Dispose();
        }
    }
}
