using System.Collections.Concurrent;
using System.Numerics;
using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;

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
        public float age;

        public static uint SizeInBytes { get; } = sizeof(float) * 4;
    }

    private ConcurrentQueue<uint> freeIndices;
    private InstancedVertexBufferObject<ParticleVertex, ParticleRenderData> buffer;
    private ParticleRenderData[] RenderData;
    private readonly ParticleVertex[] quadVerts;
    private readonly uint[] indices;

    public Material Material { get; set; }

    /// <summary>
    /// The global transform of all particles.
    /// </summary>
    public TransformComponent2D Transform { get; init; }

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
    public float ParticleSize { get; set; } = 0.05f;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleRenderer2D"/> class.
    /// </summary>
    /// <param name="material">The material.</param>
    public ParticleRenderer2D(int count, Material material)
    {
        this.Maximum = (uint)count;
        this.Material = AddEntity(material);

        Transform = AddComponent<TransformComponent2D>();

        Particles = new Particle2D[count];
        RenderData = new ParticleRenderData[count];
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
        if (!freeIndices.TryDequeue(out var index))
            return;

        input.Random = Random.Shared.NextSingle() / 5.0f + 0.5f;
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

        Parallel.For(
            0,
            Maximum,
            i =>
            {
                ref var renderData = ref RenderData[i];
                ref var particle = ref Particles[i];

                if (renderData.alive >= 0f)
                {
                    particle.Age += dt * particle.Random;
                    renderData.alive = 1.0f - particle.Age / MaxAge;

                    if (particle.Age > MaxAge)
                        renderData.alive = -5;
                    else
                        renderData.offset +=
                            particle.Direction * particle.Random * particle.Speed * dt;
                }
                else
                {
                    if (renderData.alive < -2) // floats are scary
                    {
                        renderData.alive = -1;
                        freeIndices.Enqueue((uint)i);
                        Count--;
                    }
                }
            }
        );
    }

    /// <summary>
    /// Draws the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override void Render(float dt, ref RenderOptions options)
    {
        base.Render(dt, ref options);

        if (Count < 1)
            return;

        Material.Use(in options);
        Material.SetModel(Transform.ModelMatrix);

        buffer.VertexArray.Bind();
        buffer.InstanceBuffer.BufferData(RenderData);
        buffer.VertexArray.Bind();

        unsafe
        {
            Engine
                .GL
                .DrawElementsInstanced(
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
