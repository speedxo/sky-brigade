using System.Numerics;

namespace Horizon.Input
{
    /// <summary>
    /// The MouseData struct represents input data from the mouse.
    /// </summary>
    public struct MouseData
    {
        /// <summary>
        /// Gets or sets the looking axis input as a Vector2.
        /// </summary>
        public Vector2 LookingAxis { get; set; }

        /// <summary>
        /// Gets or sets the VirtualAction representing various actions triggered by mouse input.
        /// </summary>
        public VirtualAction Actions { get; set; }
    }
}
