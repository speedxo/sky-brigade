using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering.Effects.Components
{
    public class ShaderComponent : Shader, IGameComponent
    {
        public string Name { get; set; }
        public Entity Parent { get; set; }

        public ShaderComponent(uint handle)
            : base(handle)
        {

        }

        public ShaderComponent(Shader shader)
            : base(shader.Handle)
        {

        }

        public ShaderComponent(string vertPath, string fragPath)
            : base(GameManager.Instance.ContentManager.LoadShader(vertPath, fragPath).Handle)
        {

        }
        public static ShaderComponent FromSource(string vertSource, string fragSource)
        {
            return new ShaderComponent(GameManager.Instance.ContentManager.LoadShaderFromSource(vertSource, fragSource));
        }

        public virtual void Initialize()
        {

        }

        public virtual void Update(float dt)
        {

        }

        public virtual void Draw(float dt, RenderOptions? options = null)
        {
            
        }
    }
}

