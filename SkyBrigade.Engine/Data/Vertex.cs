using System;
using System.Numerics;

namespace SkyBrigade.Engine.Data;

/*  The vertex data type is the most common in rendering engines
 *  it holds basic but neccessary information required to render
 *  any 2d or 3d scene; the vertexes coordinates in 3D worldspace
 *  which can be projected to screenspace using a MVP (model, view,
 *  projection) matrix. it also holds the UV coordinate and sometimes
 *  a unique color for the vertex (usefull for debugging).
 */
public struct Vertex
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public Vector2 TexCoords { get; set; }

    /* The SizeInBytes property is usefull for defining the vertex
     * array objects memory layout for use in shaders.
     */
    public static readonly unsafe int SizeInBytes = sizeof(Vector2) + (sizeof(Vector3) * 2);

    public Vertex(Vector3 pos, Vector3 norm, Vector2 coords)
    {
        this.TexCoords = coords;
        this.Position = pos;
        this.Normal = norm;
    }

    public Vertex(float x, float y, float z, float uvX, float uvY)
    {
        this.TexCoords = new Vector2(uvX, uvY);
        this.Position = new Vector3(x, y, z);
        this.Normal = new Vector3();
    }
}

