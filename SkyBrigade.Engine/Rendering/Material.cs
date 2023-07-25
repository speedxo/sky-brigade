using System.Numerics;
using SkyBrigade.Engine.Data;
using SkyBrigade.Engine.OpenGL;
using Shader = SkyBrigade.Engine.OpenGL.Shader;

namespace SkyBrigade.Engine.Rendering
{
    public abstract class   Material : ISerializableGameObject
    {
        public virtual Texture? Texture { get; set; }
        public virtual Vector4 Color { get; set; } = Vector4.One;
        public Shader Shader { get; protected set; }

        public abstract void Use(RenderOptions? options=null);

        public void End() => GameManager.Instance.Gl.UseProgram(0);

        public abstract void Save(string path);

        public abstract void Load(string path);
    }
}