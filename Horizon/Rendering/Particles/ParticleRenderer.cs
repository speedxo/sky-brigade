using Horizon.GameEntity;
using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Rendering.Spriting;
using System.Numerics;

namespace Horizon.Rendering.Particles;

public readonly struct Particle2D
{
    public readonly Vector2 Direction { get; }
    public readonly Vector2 InitialPosition { get; }
}

public readonly struct ParticleVertex
{
    public readonly Vector2 Position { get; init; }

    public static uint SizeInBytes { get; } = sizeof(float) * 2;
}

/// <summary>
/// A 2D instanced particle systems renderer.
/// </summary>
/// <seealso cref="Horizon.GameEntity.Entity" />
/// <seealso cref="Horizon.Rendering.Spriting.I2DBatchedRenderer&lt;Horizon.Rendering.Particles.Particle2D&gt;" />
/// <seealso cref="System.IDisposable" />
public class ParticleRenderer2D : Entity, I2DBatchedRenderer<Particle2D>, IDisposable
{
    private record struct Particle2DDefinition(Particle2D Particle, TransformComponent2D Transform);

    private List<Particle2DDefinition> Particles { get; init; }

    private InstancedVertexBufferObject<ParticleVertex, Vector2> buffer;
    public Material Material { get; set; }
    public uint Count { get; protected set; }

    public ParticleRenderer2D(Material material)
    {
        this.Material = material;

        Particles = new();
        buffer = new InstancedVertexBufferObject<ParticleVertex, Vector2>();

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
            sizeof(float) * 2,
            0
        );
        buffer.VertexAttributeDivisor(1, 1); // Each particle has its own offset.
        buffer.Unbind();
    }

    public void Add(Particle2D input)
    {
        Particles.Add(
            new Particle2DDefinition(
                input,
                new TransformComponent2D() { Position = input.InitialPosition }
            )
        );
    }

    public void Remove(Particle2D input)
    {
        // TODO: better(((
        Particles.Remove(
            Particles.Find((item) => item.Particle.InitialPosition == input.InitialPosition)
        );
    }

    public override void Draw(float dt, ref RenderOptions options)
    {
        Material.Use(in options);
        buffer.VertexArray.Bind();

        //unsafe
        {
            Engine.GL.DrawElementsInstanced(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                6,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                new IntPtr(),
                Count
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
