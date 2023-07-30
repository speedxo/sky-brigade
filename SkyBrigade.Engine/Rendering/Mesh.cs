using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using System.Globalization;
using System.Numerics;

namespace SkyBrigade.Engine.Rendering;

public class Mesh : IDisposable
{
    /*  There definetly is better way to do this
     *  TODO: somehow improve
     */
    private Vector3 pos, rot, scale;

    public Vector3 Position
    { get => pos; set { pos = value; updateModelMatrix(); } }

    public Vector3 Rotation
    { get => rot; set { rot = value; updateModelMatrix(); } }

    public Vector3 Scale
    { get => scale; set { scale = value; updateModelMatrix(); } }

    public Matrix4x4 ModelMatrix { get; private set; }
    public Material Material { get; set; }

    private void updateModelMatrix()
    {
        ModelMatrix = Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale.X, scale.Y, scale.Z) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(rot.X)) * Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y)) * Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));
    }

    public uint ElementCount { get; private set; }

    private VertexBufferObject<Vertex> vbo;

    public void Use(RenderOptions? renderOptions = null) => Material.Use(renderOptions);

    public void SetUniform(string name, float value) => Material.Shader.SetUniform(name, value);

    public void SetUniform(string name, int value) => Material.Shader.SetUniform(name, value);

    public void SetUniform(string name, Vector3 value) => Material.Shader.SetUniform(name, value);

    public void SetUniform(string name, Matrix4x4 value) => Material.Shader.SetUniform(name, value);

    public void SetUniform(string name, Vector4 value) => Material.Shader.SetUniform(name, value);

    public Mesh(Func<(ReadOnlyMemory<Vertex>, ReadOnlyMemory<uint>)> loader, Material? mat = null)
    {
        (ReadOnlyMemory<Vertex> vertices, ReadOnlyMemory<uint> indices) = loader();

        vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);

        vbo.VertexBuffer.BufferData(vertices.Span);
        vbo.ElementBuffer.BufferData(indices.Span);

        ElementCount = (uint)indices.Length;

        // Telling the VAO object how to lay out the attribute pointers
        vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
        vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
        vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

        pos = Vector3.Zero;
        scale = Vector3.One;
        rot = Vector3.Zero;
        updateModelMatrix();

        Material = mat ?? new BasicMaterial();
    }

    // dont even bother
    public static Mesh FromObj(string path)
    {
        var lines = File.ReadAllLines(path);

        List<Vertex> verts = new List<Vertex>();
        List<Vector3> colors = new List<Vector3>();
        List<Vector2> texs = new List<Vector2>();
        List<Tuple<int, int, int>> faces = new List<Tuple<int, int, int>>();

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        foreach (string line in lines)
        {
            if (line.StartsWith("v "))
            {
                // Cut off beginning of line!!! damnit
                string temp = line.Substring(2);
                float x = 0, y = 0, z = 0;

                if (temp.Count((char c) => c == ' ') == 2)
                {
                    string[] vertparts = temp.Split(' ');

                    x = (float)double.Parse(vertparts[0].Trim(), NumberStyles.Any, ci);
                    y = (float)double.Parse(vertparts[1].Trim(), NumberStyles.Any, ci);
                    z = (float)double.Parse(vertparts[2].Trim(), NumberStyles.Any, ci);
                }

                verts.Add(new Vertex(new Vector3(x, y, z), Vector3.Zero, new Vector2((float)Math.Sin(x), (float)Math.Sin(z))));
            }
            else if (line.StartsWith("f "))
            {
                string temp = line.Substring(2);

                Tuple<int, int, int> face = new Tuple<int, int, int>(0, 0, 0);

                if (temp.Count((char c) => c == ' ') == 2)
                {
                    string[] faceparts = temp.Split(' ');

                    int i1, i2, i3;

                    bool success = int.TryParse(faceparts[0], out i1);
                    success &= int.TryParse(faceparts[1], out i2);
                    success &= int.TryParse(faceparts[2], out i3);

                    if (!success)
                    {
                        GameManager.Instance.Logger.Log(Logging.LogLevel.Error, $"Error parsing face: {line}");
                    }
                    else
                    {
                        face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
                        faces.Add(face);
                    }
                }
            }
            else if (line.StartsWith("vt "))
            {
                string temp = line.Substring(3);

                float x = 0, y = 0;

                if (temp.Count((char c) => c == ' ') == 1)
                {
                    string[] texparts = temp.Split(' ');

                    x = (float)double.Parse(texparts[0].Trim(), NumberStyles.Any, ci);
                    y = (float)double.Parse(texparts[1].Trim(), NumberStyles.Any, ci);
                }

                texs.Add(new Vector2(x, y));
            }
        }

        List<uint> indices = new List<uint>();
        if (texs.Count > 0)
            for (int i = 0; i < verts.Count; i++)
                verts[i] = new Vertex(verts[i].Position, verts[i].Normal, texs[i]);

        foreach (var face in faces)
        {
            indices.Add((uint)face.Item1);
            indices.Add((uint)face.Item2);
            indices.Add((uint)face.Item3);
        }

        return new Mesh(() =>
        {
            return (verts.ToArray(), indices.ToArray());
        });
    }


    public static Mesh CreateRectangle(Material? mat = null) =>
        new(() =>
        {
            return (new Vertex[] {
                    new Vertex(-1, -1, 0, 0, 1),
                    new Vertex(1, -1, 0, 1, 1),
                    new Vertex(1, 1, 0, 1, 0),
                    new Vertex(-1, 1, 0, 0, 0)
                }, new uint[] {
                    0, 1, 3,
                    1, 2, 3
                });
        }, mat);

    // creates a sphere where the vertices are evenly spaced out, normals and texcoords are generated.
    public static Mesh CreateSphere(float radius, int vertexCount = 10, Material? mat = null)
    {
        int vertexCountSquared = vertexCount * vertexCount;
        Memory<Vertex> verts = new Vertex[vertexCountSquared];
        Memory<uint> indices = new uint[(vertexCount - 1) * (vertexCount - 1) * 6];

        for (int i = 0; i < vertexCount; i++)
        {
            for (int j = 0; j < vertexCount; j++)
            {
                // Calculate the position of the vertex
                float x = (float)Math.Sin(Math.PI * i / (vertexCount - 1)) * (float)Math.Cos(2 * Math.PI * j / (vertexCount - 1));
                float y = (float)Math.Cos(Math.PI * i / (vertexCount - 1));
                float z = (float)Math.Sin(Math.PI * i / (vertexCount - 1)) * (float)Math.Sin(2 * Math.PI * j / (vertexCount - 1));

                // Add the vertex to the memory region of vertices
                verts.Span[i * vertexCount + j] = new Vertex(new Vector3(x, y, z) * radius, new Vector3(x, y, z), new Vector2((float)j / (vertexCount - 1), (float)i / (vertexCount - 1)));
            }
        }

        // Add the indices for the triangles to the memory region of indices
        int index = 0;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            for (int j = 0; j < vertexCount - 1; j++)
            {
                indices.Span[index++] = (uint)(i * vertexCount + j);
                indices.Span[index++] = (uint)(i * vertexCount + j + 1);
                indices.Span[index++] = (uint)((i + 1) * vertexCount + j);

                indices.Span[index++] = (uint)(i * vertexCount + j + 1);
                indices.Span[index++] = (uint)((i + 1) * vertexCount + j + 1);
                indices.Span[index++] = (uint)((i + 1) * vertexCount + j);
            }
        }

        return new Mesh(() =>
        {
            return (verts, indices);
        }, mat);
    }
    
    // write code to generate a cube
    public static Mesh CreateCube(float size = 1)
    {
        List<Vertex> verts = new List<Vertex>();
        List<uint> indices = new List<uint>();

        // front
        verts.Add(new Vertex(new Vector3(-size, -size, size), new Vector3(0, 0, 1), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(size, -size, size), new Vector3(0, 0, 1), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(size, size, size), new Vector3(0, 0, 1), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(-size, size, size), new Vector3(0, 0, 1), new Vector2(0, 1)));

        // back
        verts.Add(new Vertex(new Vector3(-size, -size, -size), new Vector3(0, 0, -1), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(size, -size, -size), new Vector3(0, 0, -1), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(size, size, -size), new Vector3(0, 0, -1), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(-size, size, -size), new Vector3(0, 0, -1), new Vector2(0, 1)));

        // left
        verts.Add(new Vertex(new Vector3(-size, -size, -size), new Vector3(-1, 0, 0), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(-size, -size, size), new Vector3(-1, 0, 0), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(-size, size, size), new Vector3(-1, 0, 0), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(-size, size, -size), new Vector3(-1, 0, 0), new Vector2(0, 1)));

        // right
        verts.Add(new Vertex(new Vector3(size, -size, -size), new Vector3(1, 0, 0), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(size, -size, size), new Vector3(1, 0, 0), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(size, size, size), new Vector3(1, 0, 0), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(size, size, -size), new Vector3(1, 0, 0), new Vector2(0, 1)));

        // top
        verts.Add(new Vertex(new Vector3(-size, size, -size), new Vector3(0, 1, 0), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(size, size, -size), new Vector3(0, 1, 0), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(size, size, size), new Vector3(0, 1, 0), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(-size, size, size), new Vector3(0, 1, 0), new Vector2(0, 1)));

        // bottom
        verts.Add(new Vertex(new Vector3(-size, -size, -size), new Vector3(0, -1, 0), new Vector2(0, 0)));
        verts.Add(new Vertex(new Vector3(size, -size, -size), new Vector3(0, -1, 0), new Vector2(1, 0)));
        verts.Add(new Vertex(new Vector3(size, -size, size), new Vector3(0, -1, 0), new Vector2(1, 1)));
        verts.Add(new Vertex(new Vector3(-size, -size, size), new Vector3(0, -1, 0), new Vector2(0, 1)));

        // generate indices
        for (int i = 0; i < 6; i++)
        {
            indices.Add((uint)(i * 4));
            indices.Add((uint)(i * 4 + 1));
            indices.Add((uint)(i * 4 + 2));

            indices.Add((uint)(i * 4));
            indices.Add((uint)(i * 4 + 2));
            indices.Add((uint)(i * 4 + 3));
        }
        return new Mesh(() =>
        {
            return (verts.ToArray(), indices.ToArray());
        });
    }

    public void Draw(RenderOptions? renderOptions = null)
    {
        //if (Indices == null || Indices.Length < 1) return; // Dont render if there is nothing to render. Precious performance mmmmm

        var options = renderOptions ?? RenderOptions.Default;

        Use(options);

        SetUniform("uView", options.Camera.View);
        SetUniform("uProjection", options.Camera.Projection);
        SetUniform("uModel", ModelMatrix);
        SetUniform("camPos", options.Camera.Position);

        vbo.Bind();

        // once again, i really dont wanna make the whole method unsafe for one call
        unsafe
        {
            GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);
        }

        vbo.Unbind();

        Material.End();
    }

    public void Dispose()
    {
        vbo.Dispose();
    }
}