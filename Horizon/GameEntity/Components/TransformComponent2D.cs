using Horizon.Rendering;
using System.Numerics;

namespace Horizon.GameEntity.Components
{
    /// <summary>
    /// Represents a component that handles the 2D transformation of a game entity.
    /// </summary>
    public class TransformComponent2D : IGameComponent
    {
        public string Name { get; set; }

        /// <summary>
        /// The position of the game entity in 3D space.
        /// </summary>
        private Vector2 pos;

        /// <summary>
        /// The rotation angles of the game entity in degrees around each axis (X, Y, and Z).
        /// </summary>
        private float rot;

        /// <summary>
        /// The scale factors of the game entity along each axis (X, Y, and Z).
        /// </summary>
        private Vector2 scale = Vector2.One;

        /// <summary>
        /// Updates the model matrix based on the current position, rotation, and scale values.
        /// </summary>
        private void updateModelMatrix()
        {
            // Convert rotation angles to radians
            float radiansZ = MathHelper.DegreesToRadians(rot);

            // Create quaternions for each rotation axis
            Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, radiansZ);

            // Create the model matrix
            ModelMatrix =
                Matrix4x4.CreateScale(scale.X, scale.Y, 1.0f)
                * Matrix4x4.CreateFromQuaternion(rotation)
                * Matrix4x4.CreateTranslation(pos.X, pos.Y, 0.0f);
        }

        /// <summary>
        /// The model matrix representing the transformation of the game entity.
        /// </summary>
        public Matrix4x4 ModelMatrix { get; private set; }

        /// <summary>
        /// Gets or sets the position of the game entity in 3D space.
        /// </summary>
        public Vector2 Position
        {
            get => pos;
            set
            {
                pos = value;
                updateModelMatrix();
            }
        }

        /// <summary>
        /// Gets or sets the rotation angles of the game entity in degrees around each axis (X, Y, and Z).
        /// </summary>
        public float Rotation
        {
            get => rot;
            set
            {
                rot = value;
                updateModelMatrix();
            }
        }

        /// <summary>
        /// Gets or sets the scale factors of the game entity along each axis (X, Y, and Z).
        /// </summary>
        public Vector2 Scale
        {
            get => scale;
            set
            {
                scale = value;
                updateModelMatrix();
            }
        }

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
        public void Update(float dt) { }

        /// <summary>
        /// Draws the game entity with the current transformation.
        /// </summary>
        /// <param name="dt">The elapsed time since the last draw call.</param>
        /// <param name="options">Optional render options.</param>
        public void Draw(float dt, RenderOptions? options = null) { }
    }
}
