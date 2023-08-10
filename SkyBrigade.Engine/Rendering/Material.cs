using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.OpenGL;
using System.Numerics;
namespace SkyBrigade.Engine.Rendering
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