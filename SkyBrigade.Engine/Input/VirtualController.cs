using System.Numerics;

namespace Horizon.Input
{
    /// <summary>
    /// The VirtualController struct represents the main virtualized controller that aggregates input data from various sources.
    /// </summary>
    public struct VirtualController
    {
        /// <summary>
        /// Gets or sets the movement axis input as a Vector2.
        /// </summary>
        public Vector2 MovementAxis { get; set; }

        /// <summary>
        /// Gets or sets the looking axis input as a Vector2.
        /// </summary>
        public Vector2 LookingAxis { get; set; }

        /// <summary>
        /// Gets or sets the VirtualAction representing various actions triggered by the input devices.
        /// </summary>
        public VirtualAction Actions { get; set; }

        public bool IsPressed(VirtualAction action) => Actions.HasFlag(action);
    }
}
