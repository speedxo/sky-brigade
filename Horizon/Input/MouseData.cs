using System.Numerics;

namespace Horizon.Input
{
    /// <summary>
    /// The MouseData struct represents input data from the mouse.
    /// </summary>
    public struct MouseData
    {
        /// <summary>
        /// Gets the looking axis input as a Vector2.
        /// </summary>
        public Vector2 Direction { get; set; }

        /// <summary>
        /// Gets the absolute mouse coordinates as a Vector2.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets the VirtualAction representing various actions triggered by mouse input.
        /// </summary>
        public VirtualAction Actions { get; set; }
    }
}
