using DualSenseAPI;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using System.Numerics;

namespace Horizon.Input.Components
{
    /// <summary>
    /// The DualSenseInputManager class is responsible for handling input from a joystick/gamepad.
    /// </summary>
    public class DualSenseInputManager : IGameComponent, IDisposable
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent Entity that owns this DualSenseInputManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Gets the first connected joystick/gamepad, or null if none is connected.
        /// </summary>
        public DualSense? Controller { get; private set; }

        public bool HasController { get; private set; } = false;
        public DualSenseAPI.State.DualSenseOutputState OutputState { get; set; } =
            new DualSenseAPI.State.DualSenseOutputState();

        private DualSense? AttachController()
        {
            var controller = DualSense.EnumerateControllers().FirstOrDefault();
            if (controller == null)
                return null;

            controller.Acquire();
            controller.JoystickDeadZone = 0.1f;
            controller.BeginPolling(10);
            controller.OnStatePolled += ControllerStatePolled;

            HasController = true;
            return controller;
        }

        private void ControllerStatePolled(DualSense sender)
        {
            lock (_updateLock)
            {
                var state = sender.InputState;

                // TODO: implement keybinds for controller
                actions = VirtualAction.None;

                primaryAxis = state.LeftAnalogStick;
                secondaryAxis = state.RightAnalogStick;
                triggers = new Vector2(state.L2, state.R2);

                sender.OutputState = OutputState;
            }
        }

        private readonly object _updateLock = new();

        /// <summary>
        /// Gets the JoystickBindings representing the button-to-action mappings for the joystick.
        /// </summary>
        public JoystickBindings Bindings { get; private set; }

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

            Controller = AttachController();
        }

        /// <summary>
        /// Retrieves the current JoystickData containing input information from the joystick/gamepad.
        /// </summary>
        /// <returns>The JoystickData containing the joystick input.</returns>
        public JoystickData GetData()
        {
            lock (_updateLock)
            {
                return new JoystickData
                {
                    Actions = actions,
                    PrimaryAxis = primaryAxis,
                    SecondaryAxis = secondaryAxis,
                    Triggers = triggers
                };
            }
        }

        /// <summary>
        /// Updates the JoystickInputManager, processing input from the connected joystick/gamepad.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void UpdateState(float dt)
        {
            // Not used for joystick input.
        }

        /// <summary>
        /// Draws the JoystickInputManager, not used for joystick input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Render(float dt, ref RenderOptions options)
        {
            // Not used for joystick input.
        }

        public void Dispose()
        {
            OutputState.LeftRumble = OutputState.RightRumble = 0.0f;
            OutputState.L2Effect = OutputState.R2Effect = DualSenseAPI.TriggerEffect.Default;

            Controller?.ReadWriteOnce();
            Controller?.EndPolling();
            Controller?.Release();
        }

        public void UpdatePhysics(float dt) { }
    }
}
