using SkyBrigade.Engine.Rendering;

namespace SkyBrigade.Engine.GameEntity.Components
{
    /// <summary>
    /// IGameComponent interface represents a game component.
    /// </summary>
    public interface IGameComponent
    {
        /// <summary>
        /// Gets or sets the human readable name for this component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent entity of the game component.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Initializes the game component.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates the game component.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        void Update(float dt);

        /// <summary>
        /// Draws the game component.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        /// <param name="options">Render options (optional).</param>
        void Draw(float dt, RenderOptions? options = null);
    }
}
