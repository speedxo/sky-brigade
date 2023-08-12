using System;
using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Rendering.Effects.Components;

namespace SkyBrigade.Engine.Rendering
{
    public class Technique : Entity
	{
        public ShaderComponent Shader { get; init; }
        public UniformBufferManager BufferManager { get; init; }

        public void SetUniform(string name, object? value) => Shader.SetUniform(name, value);

		public Technique(string path, string name)
		{
            Shader = AddComponent(new ShaderComponent(
                Path.Combine(path, name + ".vert"),
                Path.Combine(path, name + ".frag")
            ));
            BufferManager = AddComponent(new UniformBufferManager(Shader));
        }

        public void Use()
        {
            Shader.Use();
            //BufferManager.Use();
        }

        public void End()
        {
            Shader.End();
        }

        public override void Draw(float dt, RenderOptions? renderOptions = null)
        {
            
        }
    }
}

