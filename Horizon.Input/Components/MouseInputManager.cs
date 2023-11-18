using System.Numerics;
using Horizon.Core.Components;
using Horizon.Core.Primitives;
using Silk.NET.Input;

namespace Horizon.Input.Components
{
    // FIXME cross static ref to Entity.Engine
    /// <summary>
    /// The MouseInputManager class is responsible for handling input from the mouse.
    /// </summary>
    public class MouseInputManager : PeripheralInputManager
    {
        /// <summary>
        /// Gets the first connected mouse, or null if none is connected.
        /// </summary>
        public static IMouse? Mouse =>
            Manager.NativeInputContext.Mice.Count > 0 ? Manager.NativeInputContext.Mice[0] : null;

        /// <summary>
        /// Gets the MouseBindings representing the button-to-action mappings for the mouse.
        /// </summary>
        public MouseBindings Bindings { get; private set; }

        private VirtualAction actions;

        private Vector2 direction,
            previousPosition,
            position;

        /// <summary>
        /// Initializes the MouseInputManager by setting the default MouseBindings.
        /// </summary>
        public MouseInputManager()
        {
            Bindings = MouseBindings.Default;
        }

        public override void Initialize() { }

        public override void SwapBuffers()
        {
            previousPosition = position;
        }

        /// <summary>
        /// Retrieves the current MouseData containing input information from the mouse.
        /// </summary>
        /// <returns>The MouseData containing the mouse input.</returns>
        public MouseData GetData()
        {
            return new MouseData
            {
                Actions = actions,
                Direction = direction,
                Position = position
            };
        }

        /// <summary>
        /// Updates the MouseInputManager, processing input from the connected mouse.
        /// </summary>
        /// <param name="dt">The time elapsed since the last update.</param>
        public override void AggregateData(float dt)
        {
            actions = VirtualAction.None;

            if (Mouse is null)
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

            position = Mouse.Position;
            direction = previousPosition - position;
        }
    }
}
