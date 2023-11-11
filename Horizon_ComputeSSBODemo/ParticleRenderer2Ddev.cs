using Horizon.GameEntity.Components;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Particles;
using Horizon.Rendering;
using System.Numerics;
using Horizon.Content;
using Silk.NET.OpenGL;
using Shader = Horizon.Content.Shader;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Horizon_ComputeSSBODemo;

[StructLayout(LayoutKind.Sequential)]
struct Particle
{
    public Vector2 Position;
    public Vector2 Direction;
    public Vector4 Color;
    public float Age;
    //public Vector3 _space;
}

public class BasicParticle2DMaterialdev : CustomMaterial
{
    public BasicParticle2DMaterialdev()
        : base(new Technique("assets/particle2d/", "basic2")) { }
}

/// <summary>
/// A batched and instanced 2D particle systems renderer.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Particles.Particle2D&gt;" />
/// <seealso cref="System.IDisposable" />
public class ParticleRenderer2Ddev : Entity, IDisposable
{
    private readonly ParticleVertex[] quadVerts;
    private readonly uint[] indices;

    private Shader spawnParticles,
        updateParticles;

    private VertexBufferObject<ParticleVertex> buffer;
    private BufferObject<Particle> particleBuffer;
    private BufferObject<int> particleIndexBuffer;

    private Particle[] particles;
    private int[] data;
    private float timer = 0;

    public Material Material { get; set; }
    public int MaximumCount { get; init; }
    public int Count { get; set; }
    public int Target;

    public TransformComponent2D Transform { get; init; }
    public float MaxAge { get; set; } = 2.5f;
    public float ParticleSize { get; set; } = 0.05f;
    public Vector2 SpawnPosition { get; set; }

    public ParticleRenderer2Ddev(int max)
    {
        //unsafe
        //{
        //    int size = sizeof(Particle);
        //    Debug.Assert(size % 16 == 0);
        //}

        this.Material = new BasicParticle2DMaterialdev();
        this.MaximumCount = Target = max;

        Transform = AddComponent<TransformComponent2D>();

        particles = new Particle[max];
        //var rand = new Random(Environment.TickCount);
        //float value;
        for (int i = 0; i < max; i++)
        {
            particles[i].Age = 59.0f;
        }

        buffer = new VertexBufferObject<ParticleVertex>();

        updateParticles = ShaderFactory.CompileFromDefinitions(
            new ShaderDefinition
            {
                Type = ShaderType.ComputeShader,
                File = ("shaders/compute_updateParticle.glsl")
            }
        );
        spawnParticles = ShaderFactory.CompileFromDefinitions(
            new ShaderDefinition
            {
                Type = ShaderType.ComputeShader,
                File = ("shaders/compute_spawnParticle.glsl")
            }
        );

        updateParticles.GetResourceIndex("particle_buffer", ProgramInterface.ShaderStorageBlock);
        updateParticles.GetResourceIndex(
            "particle_index_buffer",
            ProgramInterface.ShaderStorageBlock
        );
        spawnParticles.GetResourceIndex("particle_buffer", ProgramInterface.ShaderStorageBlock);
        spawnParticles.GetResourceIndex(
            "particle_index_buffer",
            ProgramInterface.ShaderStorageBlock
        );

        Material.Technique.Shader.GetResourceIndex(
            "particle_buffer",
            ProgramInterface.ShaderStorageBlock
        );

        particleBuffer = new(BufferTargetARB.ShaderStorageBuffer);
        particleIndexBuffer = new(BufferTargetARB.ShaderStorageBuffer);

        particleBuffer.BufferData(particles);
        data = new int[max + 1];
        Array.Clear(data);
        particleIndexBuffer.BufferData(new ReadOnlySpan<int>(data));

        updateParticles.Use();
        updateParticles.BindBuffer("particle_buffer", particleBuffer);
        updateParticles.BindBuffer("particle_index_buffer", particleIndexBuffer);
        updateParticles.End();

        spawnParticles.Use();
        spawnParticles.BindBuffer("particle_buffer", particleBuffer);
        spawnParticles.BindBuffer("particle_index_buffer", particleIndexBuffer);
        spawnParticles.End();

        quadVerts = new ParticleVertex[]
        {
            new ParticleVertex(new Vector2(-ParticleSize, -ParticleSize)),
            new ParticleVertex(new Vector2(ParticleSize, -ParticleSize)),
            new ParticleVertex(new Vector2(ParticleSize, ParticleSize)),
            new ParticleVertex(new Vector2(-ParticleSize, ParticleSize))
        };
        indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        buffer.VertexBuffer.BufferData(quadVerts);
        buffer.ElementBuffer.BufferData(indices);

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
        buffer.Unbind();
    }

    public override void UpdateState(float dt)
    {
        timer += dt;
        spawnParticles.Use();
        spawnParticles.SetUniform("uDt", dt);
        spawnParticles.SetUniform("uSpawnPosition", SpawnPosition);
        spawnParticles.SetUniform("uParticlesToSpawn", Target);
        spawnParticles.BindBuffer("particle_buffer", particleBuffer);
        spawnParticles.BindBuffer("particle_index_buffer", particleIndexBuffer);

        Engine.GL.DispatchCompute((uint)MaximumCount / 1024, 1, 1);
        Engine.GL.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);

        if (timer > 1)
        {
            spawnParticles.ReadSSBO(1, particleIndexBuffer, ref data);
            Count = MaximumCount - data[0];
            timer = 0.0f;
        }

        spawnParticles.End();

        updateParticles.Use();
        updateParticles.SetUniform("uDt", dt);
        updateParticles.SetUniform("uParticlesToSpawn", MaximumCount);
        updateParticles.BindBuffer("particle_buffer", particleBuffer);
        updateParticles.BindBuffer("particle_index_buffer", particleIndexBuffer);

        Engine.GL.DispatchCompute((uint)MaximumCount / 1024, 1, 1);
        Engine.GL.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);

        updateParticles.End();

        base.UpdateState(dt);
    }

    public override void Render(float dt, ref RenderOptions options)
    {
        Material.Use(in options);
        Material.SetModel(Transform.ModelMatrix);

        Material.Technique.Shader.BindBuffer("particle_buffer", particleBuffer);
        //Material.Technique.Shader.BindBuffer("particle_index_buffer", particleIndexBuffer);

        buffer.VertexArray.Bind();
        unsafe
        {
            Engine.GL.DrawElementsInstanced(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                6,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                null,
                (uint)MaximumCount
            );
        }

        buffer.VertexArray.Unbind();
        Material.End();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        buffer.Dispose();
    }
}
