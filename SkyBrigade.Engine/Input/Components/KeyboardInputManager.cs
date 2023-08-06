using System;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.Input.Components
{
    /// <summary>
    /// The KeyboardManager class is responsible for handling input from the keyboard.
    /// </summary>
    public class KeyboardManager : IGameComponent
    {
        /// <summary>
        /// Gets or sets the parent Entity that owns this KeyboardManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// The parent input manager
        /// </summary>
        public InputManager Manager { get; private set; }

        /// <summary>
        /// Gets the first connected keyboard, or null if none is connected.
        /// </summary>
        public static IKeyboard? Keyboard => GameManager.Instance.Input.Keyboards.Count > 0 ? GameManager.Instance.Input.Keyboards[0] : null;

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
            Manager = Parent as InputManager;
        }

        /// <summary>
        /// Retrieves the current KeyboardData containing input information from the keyboard.
        /// </summary>
        /// <returns>The KeyboardData containing the keyboard input.</returns>
        public KeyboardData GetData()
        {
            return new KeyboardData
            {
                Actions = actions,
                MovementDirection = direction
            };
        }

        /// <summary>
        /// Updates the KeyboardManager, processing input from the connected keyboard.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void Update(float dt)
        {
            actions = VirtualAction.None;

            if (Keyboard == null)
            {
                direction = default;
                actions = default;

                return;
            }

            var state = Keyboard.CaptureState();

            foreach ((Key key, VirtualAction action) in Bindings.KeyActionPairs)
            {
                if (state.IsKeyPressed(key))
                {
                    actions |= action;
                }
                else
                {
                    actions &= ~action;
                }
            }

            direction = new Vector2(
                state.IsKeyPressed(Key.D) ? 1 : state.IsKeyPressed(Key.A) ? -1 : 0,
                state.IsKeyPressed(Key.S) ? -1 : state.IsKeyPressed(Key.W) ? 1 : 0
            );
        }

        /// <summary>
        /// Draws the KeyboardManager, not used for keyboard input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Draw(float dt, RenderOptions? options = null)
        {
            // Not used for keyboard input.
        }
    }
}
