using System;
using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Rendering.Effects.Components;

namespace Horizon.Rendering
{
    public class Technique : Entity
	{
        public ShaderComponent Shader { get; set; }
        public UniformBufferManager BufferManager { get; init; }

        public void SetUniform(string name, object? value) => Shader.SetUniform(name, value);

        public static Technique Default { get; private set; } = new Technique(OpenGL.Shader.Default);

		public Technique(string path, string name)
		{
            Shader = AddComponent(new ShaderComponent(
                Path.Combine(path, name + ".vert"),
                Path.Combine(path, name + ".frag")
            ));
            BufferManager = AddComponent(new UniformBufferManager(Shader));
        }
        public Technique(Shader shader)
        {
            Shader = AddComponent(new ShaderComponent(shader));
            BufferManager = AddComponent(new UniformBufferManager(Shader));
        }

        public void Use()
        {
            Shader.Use();
            BufferManager.Use();
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

