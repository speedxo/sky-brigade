using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Horizon.Core;
using Horizon.Core.Components;
using Horizon.Core.Data;
using Horizon.Engine;
using Horizon.OpenGL;
using Horizon.OpenGL.Assets;
using Horizon.OpenGL.Descriptions;

using Silk.NET.OpenGL;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static Horizon.Rendering.Primitives.PrimitiveRenderer;

namespace Horizon.Rendering.Primitives;

public enum PrimitiveShapeType : uint
{
    Triangle = 0,
    Rectangle = 1,
    Circle = 2
}

/// <summary>
/// A value type container encapsulating the least amount of data required for rendering a shape.
/// </summary>
/// <param name="type">ShapePrimitive primitive type</param>
/// <param name="pos">Position offset</param>
/// <param name="scale">ShapePrimitive scale</param>
/// <param name="rot">ShapePrimitive rotation in degrees</param>
[StructLayout(LayoutKind.Sequential)] // explicitly set sequential layout
public struct ShapePrimitive(PrimitiveShapeType type, Vector2 pos, Vector2 scale, Vector3 colour, float rot)
{
    [VertexLayout(0, Silk.NET.OpenGL.VertexAttribPointerType.UnsignedInt)]
    private uint type = (uint)type;

    [VertexLayout(1, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
    private Vector2 position = pos;

    [VertexLayout(2, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
    private Vector2 scale = scale;

    [VertexLayout(3, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
    private float rotation = rot;

    [VertexLayout(4, Silk.NET.OpenGL.VertexAttribPointerType.Float)]
    private Vector3 colour = colour;


    public float Rotation { get => rotation; set => rotation = value; }
    public Vector2 Scale { get => scale; set => scale = value; }
    public Vector2 Position { get => position; set => position = value; }
    public uint Type { get => type; set => type = value; }
}

/// <summary>
/// Extendable primitive shape renderer. By default all shape primitive data is stored in a persistent array buffer, 
/// it is however a storage buffer with DynamicStorageBit, and by default is updated using glBufferSubData,
/// however a CreatePointer function exists returning a mapped pointer to the buffer for extending class functionality.
/// </summary>
public class PrimitiveRenderer : Entity
{
    public TransformComponent2D Transform { get; init; }
    public Matrix4x4 ViewMatrix { get; set; }
    public VertexArrayObject VertexArray { get; private set; }
    public List<ShapePrimitive> Shapes { get; init; }

    private ShapeRendererTechnique technique;
    private float timer = 0;

    protected int arrayBufferSize = 0;
    protected UploadMethod uploadMethod;
    protected float uploadTimerInterval = 1 / 60.0f;

    public enum UploadMethod : byte
    {
        Automatic,
        Manual
    }

    private class ShapeRendererTechnique : Technique
    {
        private readonly TransformComponent2D transform;

        public ShapeRendererTechnique(in TransformComponent2D transform)
        {
            SetShader(GameEngine.Instance.ObjectManager.Shaders.CreateOrGet("ShapeRendererTechnique", ShaderDescription.FromPath("shaders/primitives", "shapes")));
            this.transform = transform;
        }

        protected override void SetUniforms()
        {
            base.SetUniforms();

            SetUniform("uModel", transform.ModelMatrix);
        }
    }

    public PrimitiveRenderer() : this(UploadMethod.Automatic) { }

    public PrimitiveRenderer(in UploadMethod method)
    {
        uploadMethod = method;
        Shapes = [];

        ViewMatrix = Matrix4x4.Identity;
        Transform = AddComponent<TransformComponent2D>();
    }

    public ShapePrimitive this[int index]
    {
        get => index > 0 && index < Shapes.Count - 1 ? Shapes[index] : default;
        set
        {
            if (index < 0 || index >= Shapes.Count) return;
            Shapes[index] = value;
        }
    }

    public void Add(in ShapePrimitive shape) => Shapes.Add(shape);
    public void Remove(in ShapePrimitive shape) => Shapes.Remove(shape);

    public override unsafe void Initialize()
    {
        base.Initialize();

        technique = new ShapeRendererTechnique(Transform);
        arrayBufferSize = 4096;
        VertexArray = GameEngine.Instance.ObjectManager.VertexArrays.Create(new VertexArrayObjectDescription
        {
            Buffers = new Dictionary<VertexArrayBufferAttachmentType, BufferObjectDescription>
            {
                { VertexArrayBufferAttachmentType.ArrayBuffer, new BufferObjectDescription {
                    IsStorageBuffer = true,
                    Size = (uint)(arrayBufferSize * BufferObject.ALIGNMENT),
                    StorageMasks =
                        BufferStorageMask.MapCoherentBit // Shared buffer access
                        | BufferStorageMask.MapPersistentBit // Pointer remains valid until glUnmapBuffer
                        | BufferStorageMask.MapWriteBit // Allow writing to the buffer
                        | BufferStorageMask.DynamicStorageBit, // Allow glBufferSubData to work
                    Type = BufferTargetARB.ArrayBuffer
                } }
            }
        }).Asset;

        VertexArray.Bind();
        VertexArray[VertexArrayBufferAttachmentType.ArrayBuffer].Bind();
        VertexArray[VertexArrayBufferAttachmentType.ArrayBuffer].SetLayout<ShapePrimitive>();
        VertexArray[VertexArrayBufferAttachmentType.ArrayBuffer].Unbind();
        VertexArray.Unbind();
    }

    /// <summary>
    /// While this function attempts to return a coherent persistent pointer, it does not ensure that 
    /// </summary>
    /// <returns></returns>
    protected unsafe ShapePrimitive* CreatePointer()
    {
        return (ShapePrimitive*)VertexArray[VertexArrayBufferAttachmentType.ArrayBuffer].MapBufferRange((uint)(arrayBufferSize * BufferObject.ALIGNMENT),
                    MapBufferAccessMask.WriteBit
                        | MapBufferAccessMask.PersistentBit
                        | MapBufferAccessMask.CoherentBit);
    }

    /// <summary>
    /// Calls to BufferSubData to update vertex array.
    /// </summary>
    public void UploadAll() => VertexArray.Buffers[VertexArrayBufferAttachmentType.ArrayBuffer].NamedBufferSubData<ShapePrimitive>(CollectionsMarshal.AsSpan(Shapes));

    public override void Render(float dt, object? obj = null)
    {
        base.Render(dt, obj);

        if (uploadMethod == UploadMethod.Automatic && (timer += dt) > uploadTimerInterval)
        {
            timer = 0;
            UploadAll();
        }


        technique.Bind();
        technique.SetUniform("uView", ViewMatrix);
        VertexArray.Bind();
        GameEngine.Instance.GL.DrawArrays(Silk.NET.OpenGL.PrimitiveType.Points, 0, (uint)Shapes.Count);
        VertexArray.Unbind();
        technique.Unbind();
    }
}
