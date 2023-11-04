﻿using Box2D.NetStandard.Common;
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
public class ParticleRenderer2D : Entity, I2DBatchedRenderer<Particle2D>, IDisposable
{
    private static Random random;

    private List<Particle2D> Particles { get; init; }
    private List<Vector2> Offsets { get; init; }

    private InstancedVertexBufferObject<ParticleVertex, Vector2> buffer;
    private readonly ParticleVertex[] quadVerts;
    private readonly uint[] indices;

    public Material Material { get; set; }
    public uint Count { get; protected set; }
    public TransformComponent2D Transform { get; init; }
    public float MaxAge { get; set; } = 2.5f;
    public float ParticleSize { get; set; } = 0.05f;

    static ParticleRenderer2D()
    {
        random = new Random(Environment.TickCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleRenderer2D"/> class.
    /// </summary>
    /// <param name="material">The material.</param>
    public ParticleRenderer2D(Material material)
    {
        this.Material = material;

        Transform = AddComponent<TransformComponent2D>();

        Particles = new();
        Offsets = new();
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
    }

    /// <summary>
    /// Commits an object to be rendered.
    /// </summary>
    /// <param name="input"></param>
    public void Add(Particle2D input)
    {
        input.Random = random.NextSingle() / 5.0f + 0.5f;
        Particles.Add(input);
        Offsets.Add(input.InitialPosition);

        Count++;
    }

    /// <summary>
    /// Remove an object from management.
    /// </summary>
    /// <param name="input"></param>
    public void Remove(Particle2D input)
    {
        // TODO: better(((
        if (
            Particles.Remove(
                Particles.Find((item) => item.InitialPosition == input.InitialPosition)
            )
        )
            Count--;
        Offsets.RemoveAt(0);
    }

    /// <summary>
    /// Updates the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    public override void Update(float dt)
    {
        base.Update(dt);

        var pSpan = CollectionsMarshal.AsSpan(Particles);
        //var oSpan = CollectionsMarshal.AsSpan(Offsets);
        for (int i = 0; i < Particles.Count; i++)
        {
            pSpan[i].Age += dt * pSpan[i].Random;
            if (pSpan[i].Age > MaxAge)
            {
                Particles.RemoveAt(i);
                Offsets.RemoveAt(i);
                Count--;
                continue;
            }

            Offsets[i] += pSpan[i].Direction * dt * pSpan[i].Random * pSpan[i].Speed;
        }
    }

    /// <summary>
    /// Draws the entity and its components.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    /// <param name="options">Render options (optional).</param>
    public override void Draw(float dt, ref RenderOptions options)
    {
        if (Count < 1)
            return;

        Material.Use(in options);
        Material.SetModel(Transform.ModelMatrix);

        buffer.VertexArray.Bind();
        buffer.InstanceBuffer.BufferData(CollectionsMarshal.AsSpan(Offsets));
        buffer.VertexArray.Bind();

        unsafe
        {
            Engine.GL.DrawElementsInstanced(
                Silk.NET.OpenGL.PrimitiveType.Triangles,
                6,
                Silk.NET.OpenGL.DrawElementsType.UnsignedInt,
                null,
                Count
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
