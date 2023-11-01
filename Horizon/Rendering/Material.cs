using Horizon.GameEntity;
using Horizon.OpenGL;
using System.Numerics;

namespace Horizon.Rendering
{
    public abstract class Material : Entity
    {
        public virtual Texture? Texture { get; set; }
        public virtual Vector4 Color { get; set; } = Vector4.One;

        public Technique Technique { get; protected set; }

        public abstract void Use(in RenderOptions options);

        public static void End() => Engine.GL.UseProgram(0);
    }
}
