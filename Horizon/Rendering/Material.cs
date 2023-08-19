using Horizon.Data;
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

        public abstract void Use(RenderOptions? options = null);

        public void End() => GameManager.Instance.Gl.UseProgram(0);
    }
}