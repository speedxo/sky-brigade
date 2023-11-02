using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using System.Numerics;

namespace Horizon.Input.Components
{
    /// <summary>
    /// The KeyboardManager class is responsible for handling input from the keyboard.
    /// </summary>
    public class KeyboardManager : IGameComponent
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent Entity that owns this KeyboardManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Represents the current most up to date state of the keyboard.
        /// </summary>
        public KeyboardState? CurrentState { get; private set; }

        /// <summary>
        /// A snapshot of the state of the keyboard one frame ago.
        /// </summary>
        public KeyboardState? PreviousState { get; private set; }

        /// <summary>
        /// The parent input manager
        /// </summary>
        public InputManager Manager { get; private set; }

        /// <summary>
        /// Gets the first connected keyboard, or null if none is connected.
        /// </summary>
        public static IKeyboard? Keyboard =>
            Entity.Engine.Input.NativeInputContext.Keyboards.Count > 0
                ? Entity.Engine.Input.NativeInputContext.Keyboards[0]
                : null;

        /// <summary>
        /// Gets the KeyboardBindings representing the key-to-action mappings for the keyboard.
        /// </summary>
        public KeyboardBindings Bindings { get; private set; }

        private VirtualAction actions;
        private Vector2 direction;

        /// <summary>
        /// Initializes the KeyboardManager by setting the default KeyboardBindings.
        /// </summary>
        public void Initialize()
        {
            Bindings = KeyboardBindings.Default;
            Manager = (InputManager)Parent;

            // Delegate updating of the previous keyboard state to after the main update callback.
            Entity.Engine.OnPostUpdate += (dt) =>
            {
                PreviousState = CurrentState;
            };
            // Handle key input at the start of the frame so that the engine has the most up to date data.
            Entity.Engine.OnPreUpdate += UpdateKeys;
        }

        /// <summary>
        /// Retrieves the current KeyboardData containing input information from the keyboard.
        /// </summary>
        /// <returns>The KeyboardData containing the keyboard input.</returns>
        public KeyboardData Data { get; private set; } = default;

        /// <summary>
        /// Updates the KeyboardManager, processing input from the connected keyboard.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void UpdateKeys(float dt)
        {
            actions = VirtualAction.None;

            if (Keyboard == null)
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
            Data = new() { Actions = actions, MovementDirection = direction };
        }

        /// <summary>
        /// Returns true if a key that was not pressed last frame is pressed.
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
        /// Updates the KeyboardManager, not used for keyboard input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        public void Update(float dt)
        {
            // Not used for keyboard input.
        }

        /// <summary>
        /// Draws the KeyboardManager, not used for keyboard input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Draw(float dt, ref RenderOptions options)
        {
            // Not used for keyboard input.
        }
    }
}
