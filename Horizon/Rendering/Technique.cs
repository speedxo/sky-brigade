using Horizon.GameEntity;
using Horizon.OpenGL;
using Horizon.Content;

namespace Horizon.Rendering
{
    public class Technique : Entity, IDisposable
    {
        public Shader Shader { get; set; }
        public UniformBufferManager BufferManager { get; init; }

        public uint Handle
        {
            get => Shader.Handle;
        }

        public void SetUniform(in string name, in object? value) => Shader.SetUniform(name, value);

        public static Technique Default { get; private set; } =
            new Technique(Engine.Content.Shaders["basic"]!);

        public Technique(in string path, in string name)
        {
            Shader = AddEntity(
                ShaderFactory.CompileNamed(path, name)
            );
            BufferManager = AddComponent(new UniformBufferManager(Shader));
        }

        public Technique(in Shader shader)
        {
            Shader = AddEntity(shader);
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            BufferManager.Dispose();
            Shader.Dispose();
            Engine.GL.DeleteProgram(Handle);
        }
    }
}
