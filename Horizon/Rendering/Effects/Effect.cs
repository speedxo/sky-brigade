using Horizon.OpenGL;
using Horizon.Primitives;

namespace Horizon.Rendering.Effects
{
    public abstract class Effect : IUpdateable
    {
        public string Source { get; init; }
        public uint BindingPoint { get; internal set; }
        public bool RequiresUpdate { get; set; }

        public Effect(string source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Overriding this is for implementing more advanced automatic code generation.
        /// In order to use this you will have to ensure that you append your shader's main ShaderStage() function with _{index} so the effect stack calls it correctly.
        /// </summary>
        /// <param name="index">the</param>
        /// <example>
        /// Source.Replace("ShaderStage", $"ShaderStage_{index}");
        /// </example>
        /// <returns></returns>
        public virtual string GetReadyShaderCode(int index)
        {
            return Source.Replace("ShaderStage", $"ShaderStage_{index}");
        }

        /// <summary>
        /// Here is where you will upload your data.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="bufferObject"></param>
        public abstract void UpdateBuffer(in UniformBufferObject bufferObject);

        /// <summary>
        /// Update is called every frame
        /// </summary>
        /// <param name="dt">deltatime between the last two frames</param>
        public abstract void Update(float dt);
    }
}
