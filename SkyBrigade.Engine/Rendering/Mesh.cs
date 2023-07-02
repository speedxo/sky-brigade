using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using ObjLoader.Loader.Loaders;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering;

public class Mesh : IDisposable
{
    /*  There definetly is better way to do this
     *  TODO: somehow improve
     */
    private Vector3 pos, rot, scale;

    public Vector3 Position { get => pos; set { pos = value; updateModelMatrix(); } }
    public Vector3 Rotation { get => rot; set { rot = value; updateModelMatrix(); } }
    public Vector3 Scale { get => scale; set { scale = value; updateModelMatrix(); } }

    public Matrix4x4 ModelMatrix { get; private set; }


    private void updateModelMatrix()
    {
        ModelMatrix = Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale.X, scale.Y, scale.Z) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(rot.X)) * Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y))*Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));
    }

    public Vertex[] Vertices { get; private set; }
	public uint[] Indices { get; private set; }

	private VertexBufferObject<Vertex> vbo;

	public Mesh(Func<(Vertex[], uint[])> loader)
	{
		// yea i don't know why i went about it this way either
		(Vertex[] vertices, uint[] indices) = loader();
        Vertices = vertices;
        Indices = indices;

		// you already know the issue here
		vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);

		vbo.VertexBuffer.BufferData(Vertices);
		vbo.ElementBuffer.BufferData(Indices);

        // Telling the VAO object how to lay out the attribute pointers
        vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
        vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
        vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

        pos = Vector3.Zero;
        scale = Vector3.One;
        rot = Vector3.Zero;
        updateModelMatrix();
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
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                    else
                    {
                        face = new Tuple<int, int, int>(i1 - 1, i2 - 1, i3 - 1);
                        faces.Add(face);
                    }
                }
            }
        }

        List<uint> indices = new List<uint>();

        foreach (var face in faces)
        {
            indices.Add((uint)face.Item1);
            indices.Add((uint)face.Item2);
            indices.Add((uint)face.Item3);
        }

        return new Mesh(() => {
            return (verts.ToArray(), indices.ToArray());
        });
    }

    public static Mesh CreateRectangle() =>
        new(() => {
            return (new Vertex[] {
                    new Vertex(-1, -1, 0, 0, 1),
                    new Vertex(1, -1, 0, 1, 1),
                    new Vertex(1, 1, 0, 1, 0),
                    new Vertex(-1, 1, 0, 0, 0)
                }, new uint[] {
                    0, 1, 3,
                    1, 2, 3
                });
        });

    public void Draw(RenderOptions? renderOptions=null)
	{
        //if (Indices == null || Indices.Length < 1) return; // Dont render if there is nothing to render. Precious performance mmmmm

        var options = renderOptions ?? RenderOptions.Default;

        options.Material.Use();

        GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, options.Texture.Handle);
        options.Material.Shader.SetUniform("uTexture", 0);


        options.Material.Shader.SetUniform("uView", options.Camera.View);
        options.Material.Shader.SetUniform("uProjection", options.Camera.Projection);
        options.Material.Shader.SetUniform("uModel", ModelMatrix);
        options.Material.Shader.SetUniform("uColor", options.Color);
        options.Material.Shader.SetUniform("camPos", options.Camera.Position);


        vbo.Bind();

        // once again, i really dont wanna make the whole method unsafe for one call
        unsafe
        {
            GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        vbo.Unbind();

        options.Material.End();
    }

    public void Dispose()
    {
        vbo.Dispose();
    }
}

