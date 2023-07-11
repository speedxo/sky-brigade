using SkyBrigade.Engine.Data;
using Shader = SkyBrigade.Engine.OpenGL.Shader;

namespace SkyBrigade.Engine.Rendering
{
    public abstract class Material : ISerializableGameObject
    {
        public Shader Shader { get; protected set; }

        public abstract void Use();

        public void End() => GameManager.Instance.Gl.UseProgram(0);

        public abstract void Save(string path);

        public abstract void Load(string path);
    }
}