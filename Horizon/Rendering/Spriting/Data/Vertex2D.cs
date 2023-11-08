using System.Numerics;

namespace Horizon.Rendering.Spriting.Data
{
    public struct Vertex2D
    {
        /// <summary>
        /// Gets or sets the position of the vertex in 2D world space.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the texture coordinates (UV) of the vertex, used for texture mapping.
        /// </summary>
        public Vector2 TexCoords { get; set; }

        /// <summary>
        /// Initializes a new instance of the Vertex struct.
        /// </summary>
        /// <param name="pos">The position of the vertex in 2D world space.</param>
        /// <param name="norm">The normal vector for the vertex.</param>
        /// <param name="coords">The texture coordinates (UV) of the vertex.</param>
        public Vertex2D(Vector2 pos, Vector2 coords)
        {
            this.TexCoords = coords;
            this.Position = pos;
        }

        /// <summary>
        /// Initializes a new instance of the Vertex struct with separate coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the vertex position.</param>
        /// <param name="y">The y-coordinate of the vertex position.</param>
        /// <param name="uvX">The x-coordinate of the texture coordinates (UV).</param>
        /// <param name="uvY">The y-coordinate of the texture coordinates (UV).</param>
        public Vertex2D(float x, float y, float uvX, float uvY)
        {
            this.TexCoords = new Vector2(uvX, uvY);
            this.Position = new Vector2(x, y);
        }

        /// <summary>
        /// The size of the vertex in bytes, used for defining the vertex array object's memory layout for use in shaders.
        /// </summary>
        public static readonly int SizeInBytes = (sizeof(float) * 4);
    }
}
