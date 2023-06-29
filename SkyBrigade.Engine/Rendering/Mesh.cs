using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering;

public class Mesh
{
    /*  There definetly is better way to do this
     *  TODO: somehow improve
     */
    private Vector3 pos;
    private float rot;
    private Vector2 scale;

    public Vector3 Position { get => pos; set { pos = value; updateModelMatrix(); } }
    public float Rotation { get => rot; set { rot = value; updateModelMatrix(); } }
    public Vector2 Scale { get => scale; set { scale = value; updateModelMatrix(); } }

    public Matrix4x4 ModelMatrix { get; private set; }


    private void updateModelMatrix()
    {
        ModelMatrix = Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale.X, scale.Y, 1.0f) * Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(rot));
    }

    public ReadOnlyMemory<Vertex> Vertices { get; private set; }
	public ReadOnlyMemory<uint> Indices { get; private set; }

	private VertexBufferObject<Vertex> vbo;

	public Mesh(Func<(Vertex[], uint[])> loader)
	{
		// yea i don't know why i went about it this way either
		(Vertex[] vertices, uint[] indices) data = loader();
		Vertices = new ReadOnlyMemory<Vertex>(data.vertices);
		Indices = new ReadOnlyMemory<uint>(data.indices);

		// you already know the issue here
		vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);

        // Telling the VAO object how to lay out the attribute pointers
        vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
        vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
        vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));

		vbo.VertexBuffer.BufferData(Vertices.Span);
		vbo.ElementBuffer.BufferData(Indices.Span);
    }

	public void Draw(RenderOptions? renderOptions=null)
	{
        if (Indices.IsEmpty) return; // Dont render if there is nothing to render. Precious performance mmmmm

        var options = renderOptions ?? RenderOptions.Default;
        options.Shader.Use();

        GameManager.Instance.Gl.ActiveTexture(TextureUnit.Texture0);
        GameManager.Instance.Gl.BindTexture(TextureTarget.Texture2D, options.Texture.Handle);
        options.Shader.SetUniform("uTexture", 0);


        options.Shader.SetUniform("uView", options.Camera.View);
        options.Shader.SetUniform("uProjection", options.Camera.Projection);
        options.Shader.SetUniform("uModel", ModelMatrix);
        options.Shader.SetUniform("uColor", options.Color);

        vbo.Bind();

        // once again, i really dont wanna make the whole method unsafe for one call
        unsafe
        {
            GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }

        vbo.Unbind();
        GameManager.Instance.Gl.UseProgram(0);
    }
}

