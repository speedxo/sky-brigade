using SkyBrigade.Engine.Rendering;
using System.Numerics;

namespace SkyBrigade.Engine.GameEntity.Components
{
    /// <summary>
    /// Represents a component that handles the transformation of a game entity.
    /// </summary>
    public class TransformComponent : IGameComponent
    {
        /// <summary>
        /// The position of the game entity in 3D space.
        /// </summary>
        private Vector3 pos;

        /// <summary>
        /// The rotation angles of the game entity in degrees around each axis (X, Y, and Z).
        /// </summary>
        private Vector3 rot;

        /// <summary>
        /// The scale factors of the game entity along each axis (X, Y, and Z).
        /// </summary>
        private Vector3 scale = Vector3.One;

        /// <summary>
        /// Updates the model matrix based on the current position, rotation, and scale values.
        /// </summary>
        private void updateModelMatrix()
        {
            ModelMatrix = Matrix4x4.CreateTranslation(pos) *
                          Matrix4x4.CreateScale(scale.X, scale.Y, scale.Z) *
                          Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(rot.X)) *
                          Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y)) *
                          Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));
        }

        /// <summary>
        /// The model matrix representing the transformation of the game entity.
        /// </summary>
        public Matrix4x4 ModelMatrix { get; private set; }

        /// <summary>
        /// Gets or sets the position of the game entity in 3D space.
        /// </summary>
        public Vector3 Position
        {
            get => pos;
            set { pos = value; updateModelMatrix(); }
        }

        /// <summary>
        /// Gets or sets the rotation angles of the game entity in degrees around each axis (X, Y, and Z).
        /// </summary>
        public Vector3 Rotation
        {
            get => rot;
            set { rot = value; updateModelMatrix(); }
        }

        /// <summary>
        /// Gets or sets the scale factors of the game entity along each axis (X, Y, and Z).
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set { scale = value; updateModelMatrix(); }
        }

        /// <summary>
        /// The front direction of the game entity calculated from its rotation.
        /// </summary>
        public Vector3 Front { get => Vector3.Normalize(Rotation); }

        /// <summary>
        /// The parent entity to which this transform component belongs.
        /// </summary>
        public Entity Parent { get; set; }

        /// <summary>
        /// Initializes the transform component.
        /// </summary>
        public void Initialize()
        {
            updateModelMatrix();
        }

        /// <summary>
        /// Updates the transform component based on the elapsed time (dt).
        /// </summary>
        /// <param name="dt">The elapsed time since the last update call.</param>
        public void Update(float dt)
        {
            // TODO: Implement update logic, if needed.
        }

        /// <summary>
        /// Draws the game entity with the current transformation.
        /// </summary>
        /// <param name="dt">The elapsed time since the last draw call.</param>
        /// <param name="options">Optional render options.</param>
        public void Draw(float dt, RenderOptions? options = null)
        {
            // TODO: Implement draw logic, if needed.
        }
    }

}
