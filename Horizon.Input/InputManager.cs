using System.Numerics;
using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Horizon.Input.Components;
using Silk.NET.Input;

namespace Horizon.Input
{
    /// <summary>
    /// The InputManager class is responsible for managing input from various sources (keyboard, mouse, joystick)
    /// and providing a unified VirtualController that aggregates input data from these sources. (well said jesus)
    /// </summary>
    public class InputManager : IGameComponent
    {
        /// <summary>
        /// The window's native input context.
        /// </summary>
        public IInputContext NativeInputContext { get; private set; }

        /// <summary>
        /// All attached PeripheralInputManagers.
        /// </summary>
        public PeripheralInputManager[] Peripherals { get; init; }

        /// <summary>
        /// Gets the KeyboardInputManager responsible for handling keyboard input.
        /// </summary>
        public KeyboardInputManager KeyboardManager { get; init; }

        /// <summary>
        /// Gets the MouseInputManager responsible for handling mouse input.
        /// </summary>
        public MouseInputManager MouseManager { get; init; }

        /// <summary>
        /// Gets the XInputJoystickManager responsible for handling joystick input.
        /// </summary>
        public XInputJoystickInputManager XInputJoystickManager { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether input is captured by the InputManager.
        /// </summary>
        public bool Enabled { get; set; }

        public string Name { get; set; }
        public Entity Parent { get; set; }

        private VirtualController VirtualController = default;
        private VirtualController PreviousVirtualController = default;

        private EngineEventHandler eventHandler;

        /// <summary>
        /// Initializes a new instance of the InputManager class with default values.
        /// </summary>
        public InputManager()
        {
            // Forwards a reference to this main class to all the components.
            PeripheralInputManager.SetManager(this);

            // Attach all peripheral managers.
            Peripherals = new PeripheralInputManager[]
            {
                XInputJoystickManager = new XInputJoystickInputManager(),
                KeyboardManager = new KeyboardInputManager(),
                MouseManager = new MouseInputManager()
            };
        }

        ~InputManager()
        {
            // detach events
            eventHandler.PreState -= AggregateInputs;
            eventHandler.PostState -= SwapBuffers;
        }

        public void Initialize()
        {
            // TODO: fix assumptions
            eventHandler = Parent.GetComponent<EngineEventHandler>();

            NativeInputContext = Parent.GetComponent<WindowManager>().Input;

            // attach events
            eventHandler.PreState += AggregateInputs;
            eventHandler.PostState += SwapBuffers;

            // Initialize peripheral managers
            for (int i = 0; i < Peripherals.Length; i++)
                Peripherals[i].Initialize();
        }

        public void Render(float dt) { }

        public void UpdatePhysics(float dt) { }

        /// <summary>
        /// Swap buffers.
        /// </summary>
        private void SwapBuffers(float _)
        {
            for (int i = 0; i < Peripherals.Length; i++)
                Peripherals[i].SwapBuffers();

            PreviousVirtualController = VirtualController;
        }

        /// <summary>
        /// Aggregates all peripherals.
        /// </summary>
        private void AggregateInputs(float dt)
        {
            for (int i = 0; i < Peripherals.Length; i++)
                Peripherals[i].AggregateData(dt);
        }

        /// <summary>
        /// Updates the InputManager, aggregating input data from various sources into the VirtualController.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void UpdateState(float dt)
        {
            var keyboardData = KeyboardManager.GetData();
            var mouseData = MouseManager.GetData();
            var joystickData = XInputJoystickManager.IsConnected
                ? XInputJoystickManager.GetData()
                : JoystickData.Default;

            VirtualController.Actions =
                keyboardData.Actions | mouseData.Actions | joystickData.Actions;

            VirtualController.MovementAxis =
                keyboardData.MovementDirection
                + (XInputJoystickManager.IsConnected ? joystickData.PrimaryAxis : Vector2.Zero);

            if (VirtualController.MovementAxis.LengthSquared() > 1.0f)
                VirtualController.MovementAxis = Vector2.Normalize(VirtualController.MovementAxis);

            VirtualController.LookingAxis =
                mouseData.Direction
                + (XInputJoystickManager.IsConnected ? joystickData.SecondaryAxis : Vector2.Zero);
        }

        /// <summary>
        /// Gets the VirtualController providing unified input data from various sources.
        /// </summary>
        /// <returns>The VirtualController instance.</returns>
        public VirtualController GetVirtualController() => Enabled ? VirtualController : default;

        /// <summary>
        /// Gets the last frames VirtualController providing unified input data from various sources.
        /// </summary>
        /// <returns>The VirtualController instance.</returns>
        public VirtualController GetPreviousVirtualController() =>
            Enabled ? PreviousVirtualController : default;

        public bool IsPressed(VirtualAction action) => GetVirtualController().IsPressed(action);

        public bool WasPressed(VirtualAction action) =>
            GetVirtualController().IsPressed(action)
            && !GetPreviousVirtualController().IsPressed(action);
    }
}
