using System.Numerics;

namespace Horizon.Input
{
    /// <summary>
    /// The KeyboardData struct represents input data from the keyboard.
    /// </summary>
    public struct KeyboardData
    {
        /// <summary>
        /// Gets or sets the movement direction input as a Vector2.
        /// </summary>
        public Vector2 MovementDirection { get; set; }

        /// <summary>
        /// Gets or sets the viewing direction input as a Vector2.
        /// </summary>
        public Vector2 ViewingDirection { get; set; }

        /// <summary>
        /// Gets or sets the VirtualAction representing various actions triggered by keyboard input.
        /// </summary>
        public VirtualAction Actions { get; set; }
    }
}
