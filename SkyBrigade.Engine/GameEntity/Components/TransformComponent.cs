using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.GameEntity.Components
{
    /// <summary>
    /// TransformComponent class represents the transform of a game entity.
    /// </summary>
    public class TransformComponent : IGameComponent
    {
        /// <summary>
        /// Gets or sets the position of the game entity.
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the rotation of the game entity.
        /// </summary>
        public Vector3 Rotation { get; set; } = Vector3.Zero;

        /// <summary>
        /// Gets or sets the scale of the game entity.
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.One;

        /// <summary>
        /// Gets the front direction of the game entity.
        /// </summary>
        public Vector3 Front { get => Vector3.Normalize(Rotation); }

        /// <summary>
        /// Gets the parent entity of the transform component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Initializes the transform component.
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Updates the transform component.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        public void Update(float dt)
        {
        }

        /// <summary>
        /// Draws the transform component.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        /// <param name="options">Render options (optional).</param>
        public void Draw(float dt, RenderOptions? options = null)
        {
        }
    }
}
