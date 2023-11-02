using System.Numerics;

namespace Horizon.Rendering.Particles;

public readonly record struct ParticleVertex(Vector2 Position)
{
    public static uint SizeInBytes { get; } = sizeof(float) * 2;
}
