using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using Horizon.Core.Components;
using Horizon.Engine;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.OpenGL.Buffers;
using Horizon.OpenGL.Descriptions;
using Horizon.Rendering.Spriting;
using Silk.NET.OpenGL;

namespace Horizon.Rendering.Particles;

/// <summary>
/// A batched and instanced 2D particle systems renderer.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Particles.Particle2D&gt;" />
/// <seealso cref="System.IDisposable" />
public class ParticleRenderer2D : GameObject, IDisposable
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    private struct ParticleRenderData
    {
        public Vector2 offset;
        public float alive;
        public float _spacer;

        public static uint SizeInBytes { get; } = sizeof(float) * 4;
    }

    private ConcurrentQueue<uint> freeIndices;
    private VertexBufferObject buffer;
    private readonly ParticleVertex[] quadVerts;
    private readonly uint[] indices;
    private const string UNIFORM_CAMERA_MATRIX = "uMvp";

    public Technique Material { get; set; }

    /// <summary>
    /// The total count of all active particles.
    /// </summary>
    public uint Count { get; protected set; }

    /// <summary>
    /// The maximum age a particle can reach before it is considered dead.
    /// </summary>
    public float MaxAge { get; set; } = 2.5f;

    /// <summary>
    /// The length of the 4 sides making up the particles mesh.
    /// </summary>
    public float ParticleSize { get; set; } = 1.0f;

    /// <summary>
    /// The maximum number of particles that can exist, also the size of <see cref="Particles"/>
    /// </summary>
    public uint Maximum { get; init; }

    /// <summary>
    /// A particles initial color.
    /// </summary>
    public Vector3 StartColor { get; set; } = Vector3.One;

    /// <summary>
    /// A particles color at the end of its life.
    /// /// </summary>
    public Vector3 EndColor { get; set; } = Vector3.One;

    /// <summary>
    /// Raw CPU side particle array.
    /// </summary>
    public Particle2D[] Particles { get; init; }

    private unsafe ParticleRenderData* renderDataPtr;
    private ParticleRenderData[] renderData;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleRenderer2D"/> class.
    /// </summary>
    /// <param name="material">The material.</param>
    public ParticleRenderer2D(int count)
    {
        this.Maximum = (uint)count;
        this.Material = new Materials.BasicParticle2DTechnique(
            this,
            Engine
                .ContentManager
                .Shaders
                .CreateOrGet("particle2d", ShaderDescription.FromPath("shaders/particle", "basic"))
        );

        Particles = new Particle2D[count];
        renderData = new ParticleRenderData[count];
        freeIndices = new();

        for (uint i = 0; i < count; i++)
            freeIndices.Enqueue(i);

        quadVerts = new ParticleVertex[]
        {
            new ParticleVertex(new Vector2(-ParticleSize, -ParticleSize)),
            new ParticleVertex(new Vector2(ParticleSize, -ParticleSize)),
            new ParticleVertex(new Vector2(ParticleSize, ParticleSize)),
            new ParticleVertex(new Vector2(-ParticleSize, ParticleSize))
        };
        indices = new uint[] { 0, 1, 2, 0, 2, 3 };
    }

    public override unsafe void Initialize()
    {
        buffer = new VertexBufferObject(
            Engine
                .ContentManager
                .VertexArrays
                .Create(
                    new OpenGL.Descriptions.VertexArrayObjectDescription
                    {
                        Buffers = new()
                        {
                            {
                                VertexArrayBufferAttachmentType.ArrayBuffer,
                                BufferObjectDescription.ArrayBuffer
                            },
                            {
                                VertexArrayBufferAttachmentType.ElementBuffer,
                                BufferObjectDescription.ElementArrayBuffer
                            },
                            {
                                VertexArrayBufferAttachmentType.InstanceBuffer,
                                new BufferObjectDescription
                                {
                                    IsStorageBuffer = true,
                                    StorageMasks =
                                        Silk.NET.OpenGL.BufferStorageMask.MapCoherentBit
                                        | Silk.NET.OpenGL.BufferStorageMask.MapPersistentBit
                                        | Silk.NET.OpenGL.BufferStorageMask.MapReadBit
                                        | Silk.NET.OpenGL.BufferStorageMask.MapWriteBit,
                                    Type = Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer,
                                    Size = (uint)(Maximum * sizeof(ParticleRenderData))
                                }
                            }
                        }
                    }
                )
        );

        // Configure VAO layout
        buffer.Bind();
        buffer.VertexBuffer.Bind();
        buffer.VertexAttributePointer(
            0,
            2,
            Silk.NET.OpenGL.VertexAttribPointerType.Float,
            ParticleVertex.SizeInBytes,
            0
        );

        // Configure instance array.
        buffer.InstanceBuffer.Bind();
        buffer.VertexAttributePointer(
            1,
            2,
            Silk.NET.OpenGL.VertexAttribPointerType.Float,
            ParticleRenderData.SizeInBytes,
            0
        );
        buffer.VertexAttributePointer(
            2,
            1,
            Silk.NET.OpenGL.VertexAttribPointerType.Float,
            ParticleRenderData.SizeInBytes,
            sizeof(float) * 2
        );
        buffer.VertexAttributeDivisor(1, 1); // Each particle has its own offset.
        buffer.VertexAttributeDivisor(2, 1); // Each particle has its own offset.
        buffer.Unbind();

        buffer.VertexBuffer.NamedBufferData(quadVerts);
        buffer.ElementBuffer.NamedBufferData(indices);

        renderDataPtr = (ParticleRenderData*)
            buffer
                .InstanceBuffer
                .MapBufferRange(
                    (uint)Maximum,
                    Silk.NET.OpenGL.MapBufferAccessMask.CoherentBit
                        | Silk.NET.OpenGL.MapBufferAccessMask.WriteBit
                        | Silk.NET.OpenGL.MapBufferAccessMask.PersistentBit
                );

        base.Initialize();
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="input"></param>
    public unsafe void Add(Particle2D input)
    {
        if (!Enabled)
            return;
        if (!freeIndices.TryDequeue(out var index))
            return;

        input.Random = Random.Shared.NextSingle() / 2.0f + 0.5f;
        Particles[index] = input;
        Particles[index].Age = 0.0f;

        renderData[index].alive = 1.0f;
        renderData[index].offset = input.InitialPosition;

        Count++;
    }

    /// <summary>
    /// Updates the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    public override unsafe void UpdateState(float dt)
    {
        base.UpdateState(dt);
        if (!Enabled)
            return;

        for (int i = 0; i < Maximum; i++)
        {
            if (renderData[i].alive >= 0f)
            {
                Particles[i].Age += dt * Particles[i].Random;
                renderData[i].alive = 1.0f - Particles[i].Age / MaxAge;

                if (Particles[i].Age > MaxAge)
                    renderData[i].alive = -5;
                else
                    renderData[i].offset +=
                        Particles[i].Direction * Particles[i].Random * Particles[i].Speed * dt;
            }
            else
            {
                if (renderData[i].alive < -2) // floats are scary
                {
                    renderData[i].alive = -1;
                    freeIndices.Enqueue((uint)i);
                    Count--;
                }
            }
        }
    }

    /// <summary>
    /// Draws the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override unsafe void Render(float dt, object? obj = null)
    {
        base.Render(dt);

        if (Count < 1)
            return;

        for (int i = 0; i < Maximum; i++)
            renderDataPtr[i] = renderData[i];
        Engine.GL.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);

        Material.Bind();
        Material.SetUniform(UNIFORM_CAMERA_MATRIX, Engine.ActiveCamera.ProjView);

        buffer.Bind();
        buffer.ElementBuffer.Bind();

        unsafe
        {
            Engine
                .GL
                .DrawElementsInstanced(
                    PrimitiveType.Triangles,
                    6,
                    DrawElementsType.UnsignedInt,
                    null,
                    Maximum
                );
        }

        buffer.ElementBuffer.Unbind();
        buffer.Unbind();
        Material.Unbind();
    }
}
