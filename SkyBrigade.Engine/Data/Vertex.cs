using System.Numerics;

namespace Horizon.Data;

/*  The vertex data type is the most common in rendering engines
 *  it holds basic but neccessary information required to render
 *  any 2d or 3d scene; the vertexes coordinates in 3D worldspace
 *  which can be projected to screenspace using a MVP (model, view,
 *  projection) matrix. it also holds the UV coordinate and sometimes
 *  a unique color for the vertex (usefull for debugging).
 */

/// <summary>
/// Represents a vertex in 3D space, commonly used in rendering engines to describe geometry for 3D objects.
/// </summary>
public struct Vertex
{
    /// <summary>
    /// Gets or sets the position of the vertex in 3D world space.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the normal vector for the vertex, used in lighting calculations.
    /// </summary>
    public Vector3 Normal { get; set; }

    /// <summary>
    /// Gets or sets the texture coordinates (UV) of the vertex, used for texture mapping.
    /// </summary>
    public Vector2 TexCoords { get; set; }

    /// <summary>
    /// Initializes a new instance of the Vertex struct.
    /// </summary>
    /// <param name="pos">The position of the vertex in 3D world space.</param>
    /// <param name="norm">The normal vector for the vertex.</param>
    /// <param name="coords">The texture coordinates (UV) of the vertex.</param>
    public Vertex(Vector3 pos, Vector3 norm, Vector2 coords)
    {
        this.TexCoords = coords;
        this.Position = pos;
        this.Normal = norm;
    }

    /// <summary>
    /// Initializes a new instance of the Vertex struct with separate coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the vertex position.</param>
    /// <param name="y">The y-coordinate of the vertex position.</param>
    /// <param name="z">The z-coordinate of the vertex position.</param>
    /// <param name="uvX">The x-coordinate of the texture coordinates (UV).</param>
    /// <param name="uvY">The y-coordinate of the texture coordinates (UV).</param>
    public Vertex(float x, float y, float z, float uvX, float uvY)
    {
        this.TexCoords = new Vector2(uvX, uvY);
        this.Position = new Vector3(x, y, z);
        this.Normal = new Vector3();
    }

    /// <summary>
    /// The size of the vertex in bytes, used for defining the vertex array object's memory layout for use in shaders.
    /// </summary>
    public static readonly unsafe int SizeInBytes = sizeof(Vector2) + (sizeof(Vector3) * 2);
}