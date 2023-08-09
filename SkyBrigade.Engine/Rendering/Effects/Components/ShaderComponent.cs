using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.GameEntity.Components;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Rendering.Effects.Components
{
    public class ShaderComponent : IGameComponent
    {
        private Shader shader;

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Entity Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        // Forward some shader properties
        public uint Handle { get => shader.Handle; }
        public void SetUniform(string name, object? value) => shader.SetUniform(name, value);

        public ShaderComponent(string vertPath, string fragPath)
        {
            shader = GameManager.Instance.ContentManager.LoadShader(vertPath, fragPath);
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

