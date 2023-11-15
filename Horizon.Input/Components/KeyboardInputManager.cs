using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using System.Numerics;

namespace Horizon.Input.Components
{
    /// <summary>
    /// The KeyboardInputManager class is responsible for handling input from the keyboard.
    /// </summary>
    public class KeyboardInputManager : PeripheralInputManager
    {
        /// <summary>
        /// Represents the current most up to date state of the keyboard.
        /// </summary>
        public KeyboardState? CurrentState { get; private set; }

        /// <summary>
        /// A snapshot of the state of the keyboard one frame ago.
        /// </summary>
        public KeyboardState? PreviousState { get; private set; }

        /// <summary>
        /// Retrieves the current KeyboardData containing input information from the keyboard.
        /// </summary>
        /// <returns>The KeyboardData containing the keyboard input.</returns>
        public KeyboardData GetData()
        {
            return new()
            {
                Actions = actions,
                MovementDirection = direction
            };
        }


        /// <summary>
        /// Gets the first connected keyboard, or null if none is connected.
        /// </summary>
        public static IKeyboard? Keyboard =>
            Manager.NativeInputContext.Keyboards.Count > 0
                ? Manager.NativeInputContext.Keyboards[0]
                : null;

        /// <summary>
        /// Gets the KeyboardBindings representing the key-to-action mappings for the keyboard.
        /// </summary>
        public KeyboardBindings Bindings { get; private set; }

        private VirtualAction actions;
        private Vector2 direction;

        /// <summary>
        /// Initializes the KeyboardInputManager by setting the default KeyboardBindings.
        /// </summary>
        public KeyboardInputManager()
        {
            Bindings = KeyboardBindings.Default;
        }

        public override void Initialize()
        {

        }


        public override void SwapBuffers()
        {
            PreviousState = CurrentState;
        }

        /// <summary>
        /// Updates the KeyboardInputManager, processing input from the connected keyboard.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public override void AggregateData(float dt)
        {
            actions = VirtualAction.None;

            if (Keyboard is null)
            {
                direction = default;
                actions = default;

                return;
            }

            CurrentState = Keyboard.CaptureState();

            foreach ((Key key, VirtualAction action) in Bindings.KeyActionPairs)
            {
                if (CurrentState.IsKeyPressed(key))
                {
                    actions |= action;
                }
                else
                {
                    actions &= ~action;
                }
            }

            direction = new Vector2(
                CurrentState.IsKeyPressed(Key.D)
                    ? 1
                    : CurrentState.IsKeyPressed(Key.A)
                        ? -1
                        : 0,
                CurrentState.IsKeyPressed(Key.S)
                    ? -1
                    : CurrentState.IsKeyPressed(Key.W)
                        ? 1
                        : 0
            );
        }

        /// <summary>
        /// Returns true if a key that was not pressed last frame is currently being pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool IsKeyPressed(Key key)
        {
            if (CurrentState is null || PreviousState is null)
                return false;

            return !PreviousState.IsKeyPressed(key) && CurrentState.IsKeyPressed(key);
        }

        /// <summary>
        /// Returns the state of a key as of the current frame.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool IsKeyDown(Key key)
        {
            if (CurrentState is null)
                return false;

            return CurrentState.IsKeyPressed(key);
        }
    }
}
