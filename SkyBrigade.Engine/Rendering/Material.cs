using Microsoft.Extensions.Options;
using SkyBrigade.Engine.OpenGL;
using Shader = SkyBrigade.Engine.OpenGL.Shader;

namespace SkyBrigade.Engine.Rendering
{
    public abstract class Material
    {
        public Shader Shader { get; protected set; }
        public abstract void Use();
        public void End() => GameManager.Instance.Gl.UseProgram(0);
    }
}

