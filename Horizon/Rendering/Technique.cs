using Horizon.Content;
using Horizon.GameEntity;

namespace Horizon.Rendering
{
    /// <summary>
    /// A higher level abstraction around the <see cref="Horizon.Content.Shader"/>, providing a <see cref="UniformBufferManager"/>
    /// </summary>
    /// <seealso cref="Horizon.GameEntity.Entity" />
    /// <seealso cref="System.IDisposable" />
    public class Technique : Entity, IDisposable
    {
        /// <summary>
        /// Gets or sets the underlying shader.
        /// </summary>
        public Shader Shader { get; set; }

        /// <summary>
        /// Gets the UBO manager.
        /// </summary>
        public UniformBufferManager BufferManager { get; init; }

        /// <summary>
        /// Gets the native shader handle.
        /// </summary>
        public uint Handle
        {
            get => Shader.Handle;
        }

        /// <summary>
        /// Sets a shader uniform.
        /// </summary>
        public void SetUniform(in string name, in object? value) => Shader.SetUniform(name, value);

        public static Technique Default { get; private set; } =
            new Technique(Engine.Content.Shaders["basic"]!);

        /// <summary>
        /// Initializes a new instance of the <see cref="Technique"/> class.
        /// </summary>
        public Technique(in string path, in string name)
        {
            Shader = AddEntity(ShaderFactory.CompileNamed(path, name));
            BufferManager = AddComponent(new UniformBufferManager(Shader));
            Engine.Logger.Log(
                Logging.LogLevel.Debug,
                $"[Technique({Shader.Handle})] Created from '{name}' succesfully!"
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Technique"/> class.
        /// </summary>
        public Technique(in Shader shader)
        {
            Shader = AddEntity(shader);
            BufferManager = AddComponent(new UniformBufferManager(Shader));
            Engine.Logger.Log(
                Logging.LogLevel.Debug,
                $"[Technique({Shader.Handle})] Created succesfully!"
            );
        }

        /// <summary>
        /// Bind the program handle.
        /// </summary>
        public void Use()
        {
            Shader.Use();
            //BufferManager.Use();
        }

        /// <summary>
        /// Unbinds the program handle.
        /// </summary>
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
