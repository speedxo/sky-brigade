﻿using Microsoft.Extensions.Options;
using Silk.NET.OpenGL;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using System.Globalization;
using System.Numerics;

namespace SkyBrigade.Engine.Rendering;

/// <summary>
/// Represents an abstract class for handling meshes used in rendering.
/// </summary>
public abstract class Mesh : IDisposable
{
    /// <summary>
    /// Delegate used to obtain mesh data for the mesh.
    /// </summary>
    /// <returns>The mesh data containing vertices and elements of the mesh.</returns>
    public delegate MeshData MeshDataDelegate();

    /// <summary>
    /// The material used for rendering the mesh.
    /// </summary>
    public Material Material { get; set; }

    /// <summary>
    /// The number of elements (triangles) in the mesh.
    /// </summary>
    public uint ElementCount { get; private set; }

    /// <summary>
    /// The vertex buffer object (VBO) used to store vertex data of the mesh.
    /// </summary>
    public VertexBufferObject<Vertex> Vbo { get; private set; }

    /// <summary>
    /// Sets a uniform in the material's shader with a float value.
    /// </summary>
    public void SetUniform(string name, float value) => Material.Shader.SetUniform(name, value);

    /// <summary>
    /// Sets a uniform in the material's shader with an int value.
    /// </summary>
    public void SetUniform(string name, int value) => Material.Shader.SetUniform(name, value);

    /// <summary>
    /// Sets a uniform in the material's shader with a Vector3 value.
    /// </summary>
    public void SetUniform(string name, Vector3 value) => Material.Shader.SetUniform(name, value);

    /// <summary>
    /// Sets a uniform in the material's shader with a Matrix4x4 value.
    /// </summary>
    public void SetUniform(string name, Matrix4x4 value) => Material.Shader.SetUniform(name, value);

    /// <summary>
    /// Sets a uniform in the material's shader with a Vector4 value.
    /// </summary>
    public void SetUniform(string name, Vector4 value) => Material.Shader.SetUniform(name, value);

    /// <summary>
    /// Creates a new instance of the Mesh class.
    /// </summary>
    public Mesh()
    {
        Vbo = new VertexBufferObject<Vertex>(GameManager.Instance.Gl);

        // Telling the VAO object how to lay out the attribute pointers
        Vbo.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 0);
        Vbo.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 3 * sizeof(float));
        Vbo.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)Vertex.SizeInBytes, 6 * sizeof(float));
    }

    /// <summary>
    /// Loads the mesh with the given mesh data and material.
    /// </summary>
    /// <param name="meshData">The mesh data containing vertices and elements of the mesh.</param>
    /// <param name="mat">The material to use for rendering the mesh. If null, the default material will be used.</param>
    public virtual void Load(MeshDataDelegate meshData, Material? mat = null) => Load(meshData(), mat);

    /// <summary>
    /// Loads the mesh with the given mesh data and material.
    /// </summary>
    /// <param name="data">The mesh data containing vertices and elements of the mesh.</param>
    /// <param name="mat">The material to use for rendering the mesh. If null, the default material will be used.</param>
    public virtual void Load(MeshData data, Material? mat = null)
    {
        Vbo.VertexBuffer.BufferData(data.Vertices.Span);
        Vbo.ElementBuffer.BufferData(data.Elements.Span);

        ElementCount = (uint)data.Elements.Length;

        Material = mat ?? new EmptyMaterial();
    }

    /// <summary>
    /// Prepares the mesh for rendering with the provided render options.
    /// </summary>
    /// <param name="options">The render options containing camera and other parameters.</param>
    public virtual void Use(RenderOptions options)
    {
        Material.Use(options);

        SetUniform("uView", options.Camera.View);
        SetUniform("uProjection", options.Camera.Projection);
        //SetUniform("camPos", options.Camera.Position);
    }

    /// <summary>
    /// Draws the mesh with the provided render options.
    /// </summary>
    /// <param name="dt">The elapsed time since the last draw call.</param>
    /// <param name="renderOptions">Optional render options. If not provided, the default options will be used.</param>
    public virtual void Draw(float dt, RenderOptions? renderOptions = null)
    {
        if (ElementCount < 1) return; // Don't render if there is nothing to render to improve performance.

        var options = renderOptions ?? RenderOptions.Default;

        Use(options);

        Vbo.Bind();

        // Once again, I really don't want to make the whole method unsafe for one call.
        unsafe
        {
            GameManager.Instance.Gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);
        }

        Vbo.Unbind();

        Material.End();
    }

    /// <summary>
    /// Disposes of the mesh and its resources.
    /// </summary>
    public virtual void Dispose()
    {
        Vbo.Dispose();
    }
}
