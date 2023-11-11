using Box2D.NetStandard.Common;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Rendering.Spriting;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Horizon.Rendering.Particles;

/// <summary>
/// A batched and instanced 2D particle systems renderer.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Particles.Particle2D&gt;" />
/// <seealso cref="System.IDisposable" />
public class ParticleRenderer2D : Entity, IDisposable
{
    private struct ParticleRenderData
    {
        public Vector2 offset;
        public float alive;

        public static uint SizeInBytes { get; } = sizeof(float) * 3;
    }

    private static Random random;

    private Particle2D[] Particles { get; init; }
    private ParticleRenderData[] RenderData { get; init; }
    private Queue<uint> freeIndices;

    private InstancedVertexBufferObject<ParticleVertex, ParticleRenderData> buffer;
    private readonly ParticleVertex[] quadVerts;
    private readonly uint[] indices;

    public Material Material { get; set; }
    public uint Count { get; protected set; }
    public TransformComponent2D Transform { get; init; }
    public float MaxAge { get; set; } = 2.5f;
    public float ParticleSize { get; set; } = 0.05f;
    public uint Maximum { get; private set; }

    static ParticleRenderer2D()
    {
        random = new Random(Environment.TickCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleRenderer2D"/> class.
    /// </summary>
    /// <param name="material">The material.</param>
    public ParticleRenderer2D(int count, Material material)
    {
        this.Maximum = (uint)count;
        this.Material = material;

        Transform = AddComponent<TransformComponent2D>();

        Particles = new Particle2D[count];
        RenderData = new ParticleRenderData[count];
        freeIndices = new(count);

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

    public override void Initialize()
    {
        buffer = new();

        // Configure VAO layout
        buffer.VertexArray.Bind();
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

        buffer.VertexBuffer.BufferData(quadVerts);
        buffer.ElementBuffer.BufferData(indices);

        base.Initialize();
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="input"></param>
    public void Add(Particle2D input)
    {
        if (!freeIndices.Any())
            return;

        uint index = freeIndices.Dequeue();

        input.Random = random.NextSingle() / 5.0f + 0.5f;
        Particles[index] = input;
        Particles[index].Age = 0.0f;

        RenderData[index].alive = 1.0f;
        RenderData[index].offset = input.InitialPosition;

        Count++;
    }

    /// <summary>
    /// Updates the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    public override void UpdateState(float dt)
    {
        base.UpdateState(dt);

        // TODO: completely ditch this or at very least reuse a thread pool
        Parallel.For(
            0,
            Maximum,
            i =>
            {
                if (RenderData[i].alive >= 0.5f)
                {
                    Particles[i].Age += dt * Particles[i].Random;
                    if (Particles[i].Age > MaxAge)
                    {
                        RenderData[i].alive = -1f;
                        freeIndices.Enqueue((uint)i);
                        Count--;
                    }
                    else
                    {
                        RenderData[i].offset +=
                            Particles[i].Direction * dt * Particles[i].Random * Particles[i].Speed;
                    }
                }
            }
        );
        //for (uint i = 0; i < Maximum; i++) { }
    }

    /// <summary>
    /// Draws the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override void Render(float dt, ref RenderOptions options)
    {
        if (Count < 1)
            return;

        Material.Use(in options);
        Material.SetModel(Transform.ModelMatrix);

        buffer.VertexArray.Bind();
        buffer.InstanceBuffer.BufferData(RenderData);
        buffer.VertexArray.Bind();

        unsafe
        {
            Engine.GL.DrawElementsInstanced(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                6,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                null,
                Maximum
            );
        }

        buffer.VertexArray.Unbind();
        Material.End();
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        buffer.Dispose();
    }
}
