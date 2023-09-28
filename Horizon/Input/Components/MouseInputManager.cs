using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.Rendering;
using Silk.NET.Input;
using System.Numerics;

namespace Horizon.Input.Components
{
    // FIXME cross static ref to Entity.Engine
    /// <summary>
    /// The MouseInputManager class is responsible for handling input from the mouse.
    /// </summary>
    public class MouseInputManager : IGameComponent
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent Entity that owns this MouseInputManager component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Gets the first connected mouse, or null if none is connected.
        /// </summary>
        public static IMouse? Mouse =>
            Entity.Engine.Input.NativeInputContext.Mice.Count > 0 ? Entity.Engine.Input.NativeInputContext.Mice[0] : null;

        /// <summary>
        /// Gets the MouseBindings representing the button-to-action mappings for the mouse.
        /// </summary>
        public MouseBindings Bindings { get; private set; }

        private VirtualAction actions;
        private Vector2 direction,
            previousPosition;

        /// <summary>
        /// Initializes the MouseInputManager by setting the default MouseBindings.
        /// </summary>
        public void Initialize()
        {
            Bindings = MouseBindings.Default;
        }

        /// <summary>
        /// Retrieves the current MouseData containing input information from the mouse.
        /// </summary>
        /// <returns>The MouseData containing the mouse input.</returns>
        public MouseData GetData()
        {
            return new MouseData { Actions = actions, LookingAxis = direction };
        }

        /// <summary>
        /// Updates the MouseInputManager, processing input from the connected mouse.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public void Update(float dt)
        {
            actions = VirtualAction.None;

            if (Mouse == null)
                return;

            foreach ((MouseButton btn, VirtualAction action) in Bindings.MouseActionPairs)
            {
                if (Mouse.IsButtonPressed(btn))
                {
                    actions |= action;
                }
                else
                {
                    actions &= ~action;
                }
            }

            var position = Mouse.Position;

            direction = position - previousPosition;

            previousPosition = position;
        }

        /// <summary>
        /// Draws the MouseInputManager, not used for mouse input.
        /// </summary>
        /// <param name="dt">The time elapsed since the last draw.</param>
        /// <param name="options">Optional rendering options (not used).</param>
        public void Draw(float dt, RenderOptions? options = null)
        {
            // Not used for mouse input.
        }
    }
}
