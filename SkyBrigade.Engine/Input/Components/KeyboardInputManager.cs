using System;
using System.Numerics;
using Silk.NET.Input;
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
            if (Keyboard == null)
                return;

            foreach ((Key key, VirtualAction action) in Bindings.KeyActionPairs)
            {
                if (Keyboard.IsKeyPressed(key))
                {
                    actions |= action;
                }
                else
                {
                    actions ^= action;
                }
            }

            direction = new Vector2(
                Keyboard.IsKeyPressed(Key.D) ? 1 : Keyboard.IsKeyPressed(Key.A) ? -1 : 0,
                Keyboard.IsKeyPressed(Key.S) ? -1 : Keyboard.IsKeyPressed(Key.W) ? 1 : 0
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
