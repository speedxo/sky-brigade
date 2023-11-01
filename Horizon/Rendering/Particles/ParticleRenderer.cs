using Horizon.GameEntity.Components;
using Horizon.OpenGL;
using Horizon.Rendering.Spriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Rendering.Particles;

public readonly struct Particle2D
{
    public readonly Vector2 Direction { get; }
    public readonly Vector2 InitialPosition { get; }
}

public class ParticleRenderer2D : I2DBatchedRenderer<Particle2D>
{
    private record struct Particle2DDefinition(Particle2D Particle, TransformComponent2D Transform);
    private List<Particle2DDefinition> Particles { get; init; }

    //private Buffer<>

    public ParticleRenderer2D()
    {
        Particles = new();
    }

    public void Add(Particle2D input)
    {
        Particles.Add(
            new Particle2DDefinition(
                input,
                new TransformComponent2D() { 
                    Position = input.InitialPosition 
                })
            );
    }

    public void Draw(float dt, ref RenderOptions options)
    {
        
    }
}
