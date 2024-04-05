using System;
using System.Runtime.InteropServices;

using Bogz.Logging.Loggers;

using Horizon.Core;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;

namespace Horizon.Rendering.Mesh;

/// <summary>
/// Abstraction around a <see cref="Rendering.Material"/>, a <see cref="OpenGL.Technique"/> and a <see cref="VertexBufferObject"/>, please ensure to set the material and technique accordingly.
/// </summary>
/// <typeparam name="VertexType"></typeparam>
public abstract class Mesh<VertexType> : Entity
    where VertexType : unmanaged
{
    public readonly record struct QueuedMeshData(VertexType[] Vertices, uint[] Indices);

    private const string UNIFORM_ALBEDO = "uTexAlbedo";
    private const string UNIFORM_NORMAL = "uTexNormal";
    private const string UNIFORM_SPECULAR = "uTexSpecular";

    public Material Material { get; set; }
    public Technique Technique { get; set; }
    public bool HasUploadQueued { get; protected set; }
    public QueuedMeshData QueuedData { get; protected set; }

    public VertexBufferObject Buffer { get; protected set; }

    public uint ElementCount { get; protected set; }

    public Mesh() { }

    protected abstract VertexArrayObjectDescription ArrayDescription { get; }
    protected virtual VertexBufferObject AcquireBuffer() => new(
            GameEngine.Instance.ObjectManager.VertexArrays.Create(ArrayDescription)
        );

    public override void Initialize()
    {
        base.Initialize();
        Buffer = AcquireBuffer();
        SetBufferLayout();
    }

    protected abstract void SetBufferLayout();

    /// <summary>
    /// Called after the technique is bound but before the draw call is issued, by default binds material textures to samplers.
    /// </summary>
    protected virtual void CustomUniforms()
    {
        Material.BindAttachment(MaterialAttachment.Albedo, 0);
        Technique.SetUniform(UNIFORM_ALBEDO, 0);

        Material.BindAttachment(MaterialAttachment.Normal, 1);
        Technique.SetUniform(UNIFORM_NORMAL, 1);

        Material.BindAttachment(MaterialAttachment.Specular, 2);
        Technique.SetUniform(UNIFORM_SPECULAR, 2);
    }

    /// <summary>
    /// Called after the buffer has been bound, by default simply issues a call to glDrawElements.
    /// </summary>
    protected virtual unsafe void DrawBuffer()
    {
        GameEngine
            .Instance
            .GL
            .DrawElements(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                ElementCount,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                null
            );
    }

    public void QueueUpload(in VertexType[] vertices, in uint[] indices)
    {
        if (HasUploadQueued)
        {
            ConcurrentLogger.Instance.Log(Bogz.Logging.LogLevel.Error, "An attempt was made to queue mesh data for upload when it already had one queued; as a result it was discarded.");
            return;
        }

        HasUploadQueued = true;
        QueuedData = new QueuedMeshData(vertices, indices);
    }

    public override void Render(float dt, object? obj = null)
    {
        if (Buffer is null || Technique is null || Material is null)
            return;

        if (HasUploadQueued)
        {
            HasUploadQueued = false;

            if (ElementCount < QueuedData.Indices.Length) // dont reallocate unless we have to.
                Buffer.VertexBuffer.NamedBufferSubData(QueuedData.Vertices);
            else Buffer.VertexBuffer.NamedBufferData(QueuedData.Vertices);

            Buffer.ElementBuffer.NamedBufferData(QueuedData.Indices);

            ElementCount = (uint)QueuedData.Indices.Length;
        }

        if (ElementCount < 1) return;

        // bind shader and set uniforms
        Technique.Bind();
        CustomUniforms();

        // bind and render the vao
        Buffer.Bind();
        DrawBuffer();
        Buffer.Unbind();

        // unbind shader
        Technique.Unbind();
    }
}
